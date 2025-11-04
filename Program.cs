using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services.EMail;
using Sammlerplattform.Services.Processes;
using Sammlerplattform.Services.Processes.CollectionAreaProcesses;
using Sammlerplattform.Services.Processes.CollectionItemProcesses;
using Sammlerplattform.Services.Processes.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.Processes.PartyProcesses;
using Sammlerplattform.Services.Processes.PictureProcesses;
using Sammlerplattform.Services.Processes.PlaceProcesses;
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

builder.Services.AddScoped<IProcessPostalcode, PostalcodeProcessor>();
builder.Services.AddScoped<IProcessEra, EraProcessor>();
builder.Services.AddScoped<IProcessCollectionItemEntity, CollectionItemEntityProcessor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProcessCollectionItemPicture, CollectionItemPictureProcessor>();
builder.Services.AddScoped<IProcessProcessOfManufacture, ProcessOfManufactureProcessor>();
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
builder.Services.AddScoped<IProcessCollectionAttribute, CollectionAttributeProcessor>();
builder.Services.AddScoped<IProcessCollectionItemValue, CollectionItemValueProcessor>();
builder.Services.AddScoped<IProcessConcept, ConceptualRelationshipProcessor>();
builder.Services.AddScoped<IProcessConceptRelation, ConceptRelationProcessor>();
builder.Services.AddScoped<IProcessCollectionItemPotential, CollectionItemPotentialProcessor>();
builder.Services.AddScoped<IProcessState, StateProcessor>();
builder.Services.AddScoped<IProcessPicturePhysically, PhysicalPictureProcessor>();

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