using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.EMail;
using Sammlerplattform.Services.ML.VectorSearch;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using System.Text.Json.Serialization;
using Sammlerplattform.Services.Translation;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
    {
        // Cycles cause of self referencing tables
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpClient();
builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = ["de", "fr", "es"];
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddDbContext<DbIdentityContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbIdentityContextConnection")));

builder.Services.AddDefaultIdentity<UsingIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DbIdentityContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 12;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.AddSession();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

builder.Services.AddScoped<IProcessPostalcode, PostalcodeProcessor>();
builder.Services.AddScoped<IProcessEra, EraProcessor>();
builder.Services.AddScoped<IProcessCollectionItemEntity, CollectionItemEntityProcessor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProcessCollectionItemPicture, CollectionItemPictureProcessor>();
builder.Services.AddScoped<IProcessPlace, PlaceProcessor>();
builder.Services.AddScoped<IProcessToponymy, ToponymyProcessor>();
builder.Services.AddScoped<IProcessSettlement, SettlementProcessor>();
builder.Services.AddScoped<IProcessBodyOfWater, BodyOfWaterProcessor>();
builder.Services.AddScoped<IProcessBuilding, BuildingProcessor>();
builder.Services.AddScoped<IProcessField, FieldProcessor>();
builder.Services.AddScoped<IProcessRegion, RegionProcessor>();
builder.Services.AddScoped<IProcessRelief, ReliefProcessor>();
builder.Services.AddScoped<IProcessTransportRoute, TransportRouteProcessor>();
builder.Services.AddScoped<IProcessParty, PartyProcessor>();
builder.Services.AddScoped<IProcessIndividual, IndividualProcessor>();
builder.Services.AddScoped<IProcessOrganization, OrganizationProcessor>();
builder.Services.AddScoped<IProcessCollectionArea, CollectionAreaProcessor>();
builder.Services.AddScoped<IProcessConceptValue, ConceptValueProcessor>();
builder.Services.AddScoped<IProcessConcept, ConceptualRelationshipProcessor>();
builder.Services.AddScoped<IProcessConceptRelation, ConceptRelationProcessor>();
builder.Services.AddScoped<IProcessCollectionSet, CollectionSetProcessor>();
builder.Services.AddScoped<IProcessStatePreservation, StatePreservationProcessor>();
builder.Services.AddScoped<IProcessPicturePhysically, PhysicalPictureProcessor>();
builder.Services.AddSingleton<IEmbeddingService, SimpleEmbeddingService>();
builder.Services.AddScoped<IProcessCollectionItemEmbedding, CollectionItemEmbeddingProcessor>();
builder.Services.AddScoped<IDeeplTranslationService, DeeplTranslationService>();
builder.Services.AddScoped<IProcessTranslations, ProcessTranslations>();
builder.Services.AddScoped<ITranslationStore, TranslationStore>();
builder.Services.AddScoped<ITrackEvents, EventTracker>();
builder.Services.AddScoped<IProcessImprovementSuggestions, ImprovementSuggestionsProcessor>();

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}

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

app.Run();