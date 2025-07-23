using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ImageDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.PostcardDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;


namespace Sammlerplattform.Data;

public class DbIdentityContext(DbContextOptions<DbIdentityContext> options) : IdentityDbContext<UsingIdentityUser>(options)
{
    public DbSet<UserPicture> UserPicture { get; set; } = null!;
    public DbSet<Printing> Printing { get; set; } = null!;
    public DbSet<Manufactory> Manufactory { get; set; } = null!;
    public DbSet<City> City { get; set; } = null!;
    public DbSet<Postalcode> Postalcode { get; set; } = null!;
    public DbSet<Geography> Geography { get; set; } = null!;
    public DbSet<Person> Person { get; set; } = null!;
    public DbSet<PostcardEntity> PostcardEntity { get; set; } = null!;
    public DbSet<PostcardImprint> PostcardImprint { get; set; } = null!;
    public DbSet<ProductPicture> ProductPicture { get; set; } = null!;
    public DbSet<PostcardPotential> PostcardPotential { get; set; } = null!;
    public DbSet<Era> Era { get; set; } = null!;
    public DbSet<PostcardEntityNManufactoryNCity> PostcardEntityNManufactoryNCity { get; set; } = null!;
    public DbSet<ProductionFacility> ProductionFacility { get; set; } = null!;
    public DbSet<Brickname> Brickname { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        _ = builder.Entity<Image>()
            .HasDiscriminator<string>("Image_Type")
            .HasValue<Image>("Imagebase")
            .HasValue<Graphics>("Graphic");
        _ = builder.Entity<Image>()
            .HasDiscriminator<string>("Image_Type")
            .HasValue<Image>("Imagebase")
            .HasValue<PostcardImprint>("Imprint");
        _ = builder.Entity<PostcardPotential>()
            .HasMany(e => e.CityList)
            .WithMany(e => e.PostcardPotentialList)
            .UsingEntity(
                "PostcardPotentialNCity",
                l => l.HasOne(typeof(PostcardPotential)).WithMany().HasForeignKey("PostcardPotential_ID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                j => j.HasKey("PostcardPotential_ID", "City_ID"));
        _ = builder.Entity<City>()
            .HasMany(e => e.PostcardPotentialList)
            .WithMany(e => e.CityList)
            .UsingEntity(
                "PostcardPotentialNCity",
                l => l.HasOne(typeof(PostcardPotential)).WithMany().HasForeignKey("PostcardPotential_ID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                j => j.HasKey("PostcardPotential_ID", "City_ID"));
        _ = builder.Entity<Manufactory>()
            .HasMany(e => e.CityList)
            .WithMany(e => e.ManufactoryList)
            .UsingEntity(
                "ManufactoryNCity",
                l => l.HasOne(typeof(Manufactory)).WithMany().HasForeignKey("ManufactoryID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("CityID"),
                j => j.HasKey("ManufactoryID", "CityID"));
        _ = builder.Entity<City>()
            .HasMany(e => e.ManufactoryList)
            .WithMany(e => e.CityList)
            .UsingEntity(
                "ManufactoryNCity",
                l => l.HasOne(typeof(Manufactory)).WithMany().HasForeignKey("ManufactoryID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("CityID"),
                j => j.HasKey("ManufactoryID", "CityID"));
        _ = builder.Entity<Geography>()
            .HasMany(e => e.CityICollection)
            .WithOne(e => e.Geography)
            .HasForeignKey(e => e.GeographyID)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasOne(e => e.Geography)
            .WithMany(e => e.CityICollection)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasMany(e => e.CityNOeconymList)
            .WithOne(e => e.City)
            .HasForeignKey(e => e.City_ID);
        _ = builder.Entity<Oeconym>()
            .HasMany(e => e.CityNOeconymList)
            .WithOne(e => e.Oeconym)
            .HasForeignKey(e => e.Oeconym_ID);
        _ = builder.Entity<City>()
            .HasOne(c => c.ParentCity)
            .WithMany(c => c.ChildCity)
            .HasForeignKey(c => c.ParentCityID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        _ = builder.Entity<City>()
            .HasMany(e => e.PostalcodeList)
            .WithMany(e => e.CityICollection)
            .UsingEntity(
                "CityNPostalcode",
                l => l.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                r => r.HasOne(typeof(Postalcode)).WithMany().HasForeignKey("Postalcode_ID"),
                j => j.HasKey("City_ID", "Postalcode_ID"));
        _ = builder.Entity<Postalcode>()
            .HasMany(e => e.CityICollection)
            .WithMany(e => e.PostalcodeList)
            .UsingEntity(
                "CityNPostalcode",
                l => l.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                r => r.HasOne(typeof(Postalcode)).WithMany().HasForeignKey("Postalcode_ID"),
                j => j.HasKey("City_ID", "Postalcode_ID"));
        _ = builder.Entity<City>()
            .HasMany(p => p.PersonList)
            .WithOne(p => p.City)
            .HasForeignKey(p => p.City_ID)
            .IsRequired(false);
        _ = builder.Entity<ProductionFacility>()
            .HasMany(x => x.ManufactoryICollection)
            .WithOne(x => x.ProductionFacility)
            .HasForeignKey(x => x.ProductionFacility_ID)
            .IsRequired(false);
        _ = builder.Entity<Manufactory>()
            .HasOne(x => x.ProductionFacility)
            .WithMany(x => x.ManufactoryICollection)
            .IsRequired(false);
        //_ = builder.Entity<BrickEntity>()
        //    .HasOne(x => x.ManufacturingDate)
        //    .WithMany(x => x.BrickEntityICollection)
        //    .IsRequired(false);
        //_ = builder.Entity<ManufacturingDate>()
        //    .HasMany(x => x.BrickEntityICollection)
        //    .WithOne(x => x.ManufacturingDate)
        //    .HasForeignKey(x => x.ManufacturingDate_ID)
        //    .IsRequired(false);
        //_ = builder.Entity<PostcardEntity>()
        //    .HasOne(x => x.ManufacturingDate)
        //    .WithMany(x => x.PostcardEntityICollection)
        //    .IsRequired(false);
        //_ = builder.Entity<ManufacturingDate>()
        //    .HasMany(x => x.PostcardEntityICollection)
        //    .WithOne(x => x.ManufacturingDate)
        //    .HasForeignKey(x => x.ManufacturingDate_ID)
        //    .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.UsingIdentityUser)
            .WithMany(x => x.BrickEntityICollection);
        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(x => x.BrickEntityICollection)
            .WithOne(x => x.UsingIdentityUser)
            .HasForeignKey(x => x.UsingIdentityUsersID);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.Era)
            .WithMany(x => x.BrickEntityList);
        _ = builder.Entity<Era>()
            .HasMany(x => x.BrickEntityList)
            .WithOne(x => x.Era)
            .HasForeignKey(x => x.EraId);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.ProcessOfManufacture)
            .WithMany(x => x.BrickEntityList);
        _ = builder.Entity<ProcessOfManufacture>()
            .HasMany(x => x.BrickEntityList)
            .WithOne(x => x.ProcessOfManufacture)
            .HasForeignKey(x => x.ProcessOfManufactureID);
        //_ = builder.Entity<ManufacturingDate>()
        //    .HasOne(x => x.Era)
        //    .WithMany(x => x.ManufacturingDateICollection)
        //    .IsRequired(false);
        //_ = builder.Entity<Era>()
        //    .HasMany(x => x.ManufacturingDateICollection)
        //    .WithOne(x => x.Era)
        //    .HasForeignKey(x => x.Era_ID)
        //    .IsRequired(false);
        _ = builder.Entity<Person>()
            .HasMany(e => e.PrizeICollection)
            .WithMany(e => e.PersonICollection)
            .UsingEntity(
                "PersonNPrize",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Prize)).WithMany().HasForeignKey("Prize_ID"),
                j => j.HasKey("Person_ID", "Prize_ID"));
        _ = builder.Entity<Prize>()
            .HasMany(e => e.PersonICollection)
            .WithMany(e => e.PrizeICollection)
            .UsingEntity(
                "PersonNPrize",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Prize)).WithMany().HasForeignKey("Prize_ID"),
                j => j.HasKey("Person_ID", "Prize_ID"));
        _ = builder.Entity<Person>()
            .HasMany(e => e.ProfessionICollection)
            .WithMany(e => e.PersonICollection)
            .UsingEntity(
                "PersonNProfession",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Profession)).WithMany().HasForeignKey("Profession_ID"),
                j => j.HasKey("Person_ID", "Profession_ID"));
        _ = builder.Entity<Profession>()
            .HasMany(e => e.PersonICollection)
            .WithMany(e => e.ProfessionICollection)
            .UsingEntity(
                "PersonNProfession",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Profession)).WithMany().HasForeignKey("Profession_ID"),
                j => j.HasKey("Person_ID", "Profession_ID"));
        _ = builder.Entity<BrickPotential>()
            .HasMany(x => x.BricknameSynonymList)
            .WithOne(x => x.BrickPotential)
            .HasForeignKey(x => x.BrickPotentialID);
        _ = builder.Entity<Brickname>()
            .HasOne(x => x.BrickPotential)
            .WithMany(x => x.BricknameSynonymList);
        //_ = builder.Entity<BrickPotential>()
        //    .HasOne(c => c.BrickPotentialGeneric)
        //    .WithMany(c => c.BrickPotentialSpeciesICollection)
        //    .HasForeignKey(c => c.BrickPotentialGeneric_ID)
        //    .IsRequired(false)
        //.OnDelete(DeleteBehavior.Restrict);
        //_ = builder.Entity<BrickPotential>()
        //    .HasMany(e => e.BrickPotentialGeneric)
        //    .WithMany(e => e.BrickPotentialSpeciesICollection)
        //    .UsingEntity(
        //        "BrickPotentialNBrickPotential",
        //        l => l.HasOne(typeof(BrickPotential)).WithMany().HasForeignKey("BrickPotentialGeneric_ID"),
        //        r => r.HasOne(typeof(BrickPotential)).WithMany().HasForeignKey("BrickPotentialSpecies_ID"),
        //        j =>
        //        {
        //            _ = j.Property<int>("BrickPotentialGeneric_ID");
        //            _ = j.Property<int>("BrickPotentialSpecies_ID");
        //            _ = j.HasKey("BrickPotentialGeneric_ID", "BrickPotentialSpecies_ID");
        //        });
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.BrickPotential)
            .WithMany(x => x.BrickEntityList);
        _ = builder.Entity<BrickPotential>()
            .HasMany(x => x.BrickEntityList)
            .WithOne(x => x.BrickPotential)
            .HasForeignKey(x => x.BrickPotentialID);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.ProductPictureList)
            .WithOne(pp => pp.BrickEntity)
            .HasForeignKey(pp => pp.BrickEntityID);
        _ = builder.Entity<ProductPicture>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.ProductPictureList);
        _ = builder.Entity<PostcardEntity>()
            .HasMany(x => x.ProductPictureList)
            .WithOne(pp => pp.PostcardEntity)
            .HasForeignKey(pp => pp.PostcardEntityID);
        _ = builder.Entity<ProductPicture>()
            .HasOne(x => x.PostcardEntity)
            .WithMany(x => x.ProductPictureList);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.BrickEntityNManufactoryNCityList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<Manufactory>()
            .HasMany(c => c.BrickEntityNManufactoryNCityList)
            .WithOne(x => x.Manufactory)
            .HasForeignKey(x => x.ManufactoryID);

        _ = builder.Entity<City>()
            .HasMany(c => c.BrickEntityNManufactoryNCityList)
            .WithOne(x => x.City)
            .HasForeignKey(x => x.CityID);
        _ = builder.Entity<BrickEntityNManufactoryNCity>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.BrickEntityNManufactoryNCityList)
            .IsRequired(true);
        _ = builder.Entity<BrickEntityNManufactoryNCity>()
            .HasOne(x => x.Manufactory)
            .WithMany(x => x.BrickEntityNManufactoryNCityList)
            .IsRequired(true);
        _ = builder.Entity<BrickEntityNManufactoryNCity>()
            .HasOne(x => x.City)
            .WithMany(x => x.BrickEntityNManufactoryNCityList);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.BrickEntityNPersonList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<Person>()
            .HasMany(c => c.BrickEntityNPersonList)
            .WithOne(x => x.Person)
            .HasForeignKey(x => x.PersonID);
        _ = builder.Entity<BrickEntityNPerson>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.BrickEntityNPersonList)
            .IsRequired(true);
        _ = builder.Entity<BrickEntityNPerson>()
            .HasOne(x => x.Person)
            .WithMany(x => x.BrickEntityNPersonList)
            .IsRequired(true);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.BrickEntityNCityList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<City>()
            .HasMany(c => c.BrickEntityNCityList)
            .WithOne(x => x.City)
            .HasForeignKey(x => x.CityID);
        _ = builder.Entity<BrickEntityNCity>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.BrickEntityNCityList)
            .IsRequired(true);
        _ = builder.Entity<BrickEntityNCity>()
            .HasOne(x => x.City)
            .WithMany(x => x.BrickEntityNCityList)
            .IsRequired(true);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.ProductNColorVariantList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<Color>()
            .HasMany(c => c.ProductNColorVariantList)
            .WithOne(x => x.Color)
            .HasForeignKey(x => x.ColorID);
        _ = builder.Entity<ProductNColorVariant>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.ProductNColorVariantList)
            .IsRequired(true);
        _ = builder.Entity<ProductNColorVariant>()
            .HasOne(x => x.Color)
            .WithMany(x => x.ProductNColorVariantList)
            .IsRequired(true);
        _ = builder.Entity<PostcardEntity>()
            .HasMany(x => x.ProductNColorVariantList)
            .WithOne(x => x.PostcardEntity)
            .HasForeignKey(x => x.PostcardEntity_ID);
        _ = builder.Entity<ProductNColorVariant>()
            .HasOne(x => x.PostcardEntity)
            .WithMany(x => x.ProductNColorVariantList)
            .IsRequired(true);
    }
}

