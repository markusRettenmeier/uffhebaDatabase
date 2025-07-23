using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.EMail;
using Sammlerplattform.Services.Processes;
using Sammlerplattform.Services.Processes.CityProcesses;
using Sammlerplattform.Services.UnitOfWork;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            // Cycles cause of self referencing tables
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SubscribedDiskspacePolicy", policy => policy.RequireClaim("SubscribedDiskspace"))
    .AddPolicy("SubscribedAnalysisToolPolicy", policy => policy.RequireClaim("SubscribedAnalysisTool"));

builder.Services.AddScoped<IProcessGeography, GeographyProcessor>();
builder.Services.AddScoped<IProcessOeconym, OeconymProcessor>();
builder.Services.AddScoped<IProcessPostalcode, PostalcodeProcessor>();
builder.Services.AddScoped<IProcessEra, EraProcessor>();
builder.Services.AddScoped<IProcessCity, CityProcessor>();
builder.Services.AddScoped<IProcessCityNOeconym, CityNOeconymProcessor>();
builder.Services.AddScoped<IProcessManufactory, ManufactoryProcessor>();
builder.Services.AddScoped<IProcessBrick, BrickProcessor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProcessProductPicture, ProductPictureProcessor>();
builder.Services.AddScoped<IProcessPerson, PersonProcessor>();
builder.Services.AddScoped<IProcessProcessOfManufacture, ProcessOfManufactureProcessor>();
builder.Services.AddScoped<UserAccessor>();

//builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
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
        pattern: "{controller=Home}/{action=Frontpage}/{id?}");

app.Run();