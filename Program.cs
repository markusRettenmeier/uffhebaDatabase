using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PasskeyProcessees;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using Sammlerplattform.Services.EMail;
using Sammlerplattform.Services.ML.VectorSearch;
using Sammlerplattform.Services.Passkey;
using Sammlerplattform.Services.Translation;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Bei Development HTTPS nicht erzwingen
if (builder.Environment.IsDevelopment())
{
    services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 5001;
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    });
}
else
{
    // In Produktion HTTPS erzwingen
    services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    });
}

services.AddMvc()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
    {
        // Cycles cause of self referencing tables
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
services.AddHttpClient();
services.AddResponseCaching();
services.AddHttpContextAccessor();

services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = ["en", "de", "fr", "es", "zh-Hans", "ja"];
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

services.AddDbContext<DbIdentityContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbIdentityContextConnection")));

services.AddDataProtection()
    .SetApplicationName("Sammlerplattform")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Sammlerplattform",
        "DataProtection-Keys"
    )));
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireRecentPasskey", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new RecentPasskeyRequirement(300)); // 5 Minuten
    });


services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.Cookie.Name = ".Sammlerplattform.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});
services.Configure<Fido2Configuration>(builder.Configuration.GetSection("Fido2"));
services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IOptions<Fido2Configuration>>().Value;
    return new Fido2(new Fido2Configuration()
    {
        ServerDomain = config.ServerDomain,
        ServerName = config.ServerName,
        Origins = config.Origins,
        TimestampDriftTolerance = config.TimestampDriftTolerance
    });
});

services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

services.AddScoped<IProcessEra, EraProcessor>();
services.AddScoped<IProcessCollectionItemEntity, CollectionItemEntityProcessor>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IProcessCollectionItemPicture, CollectionItemPictureProcessor>();
//services.AddScoped<IProcessOwnershipProofPicture, OwnershipProofPictureProcessor>();
services.AddScoped<IProcessPlace, PlaceProcessor>();
services.AddScoped<IProcessToponymy, ToponymyProcessor>();
services.AddScoped<IProcessParticpant, ParticipantProcessor>();
services.AddScoped<IProcessIndividual, IndividualProcessor>();
services.AddScoped<IProcessOrganization, OrganizationProcessor>();
services.AddScoped<IProcessIndustry, IndustryProcessor>();
services.AddScoped<IProcessCollectionArea, CollectionAreaProcessor>();
services.AddScoped<IProcessConceptValue, ConceptValueProcessor>();
services.AddScoped<IProcessConcept, ConceptualRelationshipProcessor>();
services.AddScoped<IProcessConceptRelation, ConceptRelationProcessor>();
services.AddScoped<IProcessStatePreservation, StatePreservationProcessor>();
services.AddScoped<IProcessPicturePhysically, PhysicalPictureProcessor>();
services.AddSingleton<IEmbeddingService, SimpleEmbeddingService>();
services.AddScoped<IProcessCollectionItemEmbedding, CollectionItemEmbeddingProcessor>();
services.AddScoped<IDeeplTranslationService, DeeplTranslationService>();
services.AddScoped<IProcessTranslations, ProcessTranslations>();
services.AddScoped<ITranslationStore, TranslationStore>();
services.AddScoped<ITrackEventsCSV, EventTracker>();
services.AddScoped<IProcessImprovementSuggestions, ImprovementSuggestionsProcessor>();
services.AddScoped<IProcessFidoCredential, FidoCredentialProcessor>();
services.AddSingleton<IAuthorizationHandler, RecentPasskeyHandler>();
services.AddScoped<IProcessCIRelationship, CIRelationshipProcessor>();

services.AddIdentity<UsingIdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DbIdentityContext>()
    .AddDefaultTokenProviders();

// Configure cookie behavior after AddIdentity so AddIdentity doesn't overwrite these settings
builder.Services.ConfigureApplicationCookie(options =>
{
    // Redirect unauthenticated users to the passkey login page
    options.LoginPath = "/Passkey/Login";
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"/Passkey/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    };

    // Redirect users who are authenticated but forbidden to a passkey page with status info
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Passkey/Login?StatusCode=403&StatusMessage=Error_Access_Forbidden");
        return Task.CompletedTask;
    };
});

services.Configure<AuthMessageSenderOptions>(builder.Configuration);

var keyPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("Sammlerplattform");

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}
//app.MapGet("/env", (IConfiguration config, IWebHostEnvironment env) =>
//{
//    return Results.Json(new
//    {
//        Environment = env.EnvironmentName,
//        FidoDomain = config["Fido2:ServerDomain"]
//    });
//});

app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Frontpage}");

app.Use((context, next) =>
{
    // Füge Link-Header nur für die Startseite hinzu
    if (context.Request.Path == "/")
    {
        // RFC 8288 konformer Link-Header
        // Mehrere Links werden durch Kommas getrennt
        context.Response.Headers.Append(
            "Link",
            "</.well-known/api-catalog>; rel=\"api-catalog\", " +
            "</docs/api>; rel=\"service-doc\", " +
            "<https://uffheba.online/Home/Details>; rel=\"contents\""
        );
    }
    return next();
});

app.Use(async (context, next) =>
{
    // Prüfen, ob der Client Markdown möchte
    if (context.Request.Headers.Accept.ToString().Contains("text/markdown"))
    {
        // Ursprünglichen Response-Stream speichern
        var originalStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(); // HTML wird normal generiert

        // HTML aus dem Stream lesen
        memoryStream.Seek(0, SeekOrigin.Begin);
        var html = await new StreamReader(memoryStream).ReadToEndAsync();

        // HTML zu Markdown konvertieren (z. B. mit ReverseMarkdown)
        var config = new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        };
        var converter = new ReverseMarkdown.Converter(config);
        var markdown = converter.Convert(html);

        // Response anpassen
        context.Response.Body = originalStream;
        context.Response.ContentType = "text/markdown";
        int estimateCount = markdown.Length / 4; // Grobe Schätzung: 1 Token ≈ 4 Zeichen
        context.Response.Headers["x-markdown-tokens"] = estimateCount.ToString();
        await context.Response.WriteAsync(markdown);
    }
    else
    {
        await next(); // Normale HTML-Antwort
    }
});

app.Run();