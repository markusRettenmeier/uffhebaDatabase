using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
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
    public DbSet<Image> Image { get; set; } = null!;
    public DbSet<Oeconym> Oeconym { get; set; } = null!;
    public DbSet<PostcardEntityNManufactoryNCity> PostcardEntityNManufactoryNCity { get; set; } = null!;
    public DbSet<ProductionFacility> ProductionFacility { get; set; } = null!;
    public DbSet<Prize> Prize { get; set; } = null!;
    public DbSet<Profession> Profession { get; set; } = null!;
    public DbSet<Brickname> Brickname { get; set; } = null!;
    public DbSet<BrickPotential> BrickPotential { get; set; } = null!;
    public DbSet<BrickEntity> BrickEntity { get; set; } = null!;
    public DbSet<ManufacturingDate> ManufacturingDate { get; set; } = null!;

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
            .HasMany(e => e.CityICollection)
            .WithMany(e => e.ManufactoryList)
            .UsingEntity(
                "ManufactoryNCity",
                l => l.HasOne(typeof(Manufactory)).WithMany().HasForeignKey("Manufactory_ID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                j => j.HasKey("Manufactory_ID", "City_ID"));
        _ = builder.Entity<City>()
            .HasMany(e => e.ManufactoryList)
            .WithMany(e => e.CityICollection)
            .UsingEntity(
                "ManufactoryNCity",
                l => l.HasOne(typeof(Manufactory)).WithMany().HasForeignKey("Manufactory_ID"),
                r => r.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                j => j.HasKey("Manufactory_ID", "City_ID"));
        _ = builder.Entity<Geography>()
            .HasMany(e => e.CityICollection)
            .WithOne(e => e.Geography)
            .HasForeignKey(e => e.Geography_ID)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasOne(e => e.Geography)
            .WithMany(e => e.CityICollection)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasMany(e => e.CityNOeconymICollection)
            .WithOne(e => e.City)
            .HasForeignKey(e => e.City_ID);
        _ = builder.Entity<Oeconym>()
            .HasMany(e => e.CityNOeconymICollection)
            .WithOne(e => e.Oeconym)
            .HasForeignKey(e => e.Oeconym_ID);
        _ = builder.Entity<City>()
            .HasOne(c => c.ParentCity)
            .WithMany(c => c.ChildCity)
            .HasForeignKey(c => c.ParentCity_ID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        _ = builder.Entity<City>()
            .HasMany(e => e.PostalcodeICollection)
            .WithMany(e => e.CityICollection)
            .UsingEntity(
                "CityNPostalcode",
                l => l.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                r => r.HasOne(typeof(Postalcode)).WithMany().HasForeignKey("Postalcode_ID"),
                j => j.HasKey("City_ID", "Postalcode_ID"));
        _ = builder.Entity<Postalcode>()
            .HasMany(e => e.CityICollection)
            .WithMany(e => e.PostalcodeICollection)
            .UsingEntity(
                "CityNPostalcode",
                l => l.HasOne(typeof(City)).WithMany().HasForeignKey("City_ID"),
                r => r.HasOne(typeof(Postalcode)).WithMany().HasForeignKey("Postalcode_ID"),
                j => j.HasKey("City_ID", "Postalcode_ID"));
        _ = builder.Entity<City>()
            .HasMany(p => p.PersonICollection)
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
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.ManufacturingDate)
            .WithMany(x => x.BrickEntityICollection)
            .IsRequired(false);
        _ = builder.Entity<ManufacturingDate>()
            .HasMany(x => x.BrickEntityICollection)
            .WithOne(x => x.ManufacturingDate)
            .HasForeignKey(x => x.ManufacturingDate_ID)
            .IsRequired(false);
        _ = builder.Entity<PostcardEntity>()
            .HasOne(x => x.ManufacturingDate)
            .WithMany(x => x.PostcardEntityICollection)
            .IsRequired(false);
        _ = builder.Entity<ManufacturingDate>()
            .HasMany(x => x.PostcardEntityICollection)
            .WithOne(x => x.ManufacturingDate)
            .HasForeignKey(x => x.ManufacturingDate_ID)
            .IsRequired(false);
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
            .HasMany(x => x.BricknameSynonymICollection)
            .WithOne(x => x.BrickPotential)
            .HasForeignKey(x => x.BrickPotential_ID);
        _ = builder.Entity<Brickname>()
            .HasOne(x => x.BrickPotential)
            .WithMany(x => x.BricknameSynonymICollection);
        //_ = builder.Entity<BrickPotential>()
        //    .HasOne(c => c.BrickPotentialGeneric)
        //    .WithMany(c => c.BrickPotentialSpeciesICollection)
        //    .HasForeignKey(c => c.BrickPotentialGeneric_ID)
        //    .IsRequired(false)
        //.OnDelete(DeleteBehavior.Restrict);
        _ = builder.Entity<BrickPotential>()
            .HasMany(e => e.BrickPotentialGeneric)
            .WithMany(e => e.BrickPotentialSpeciesICollection)
            .UsingEntity(
                "BrickPotentialNBrickPotential",
                l => l.HasOne(typeof(BrickPotential)).WithMany().HasForeignKey("BrickPotentialGeneric_ID"),
                r => r.HasOne(typeof(BrickPotential)).WithMany().HasForeignKey("BrickPotentialSpecies_ID"),
                j =>
                {
                    j.Property<int>("BrickPotentialGeneric_ID");
                    j.Property<int>("BrickPotentialSpecies_ID");
                    j.HasKey("BrickPotentialGeneric_ID", "BrickPotentialSpecies_ID");
                });
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.BrickPotential)
            .WithMany(x => x.BrickEntityICollection)
            .IsRequired(false);
        _ = builder.Entity<BrickPotential>()
            .HasMany(x => x.BrickEntityICollection)
            .WithOne(x => x.BrickPotential)
            .HasForeignKey(x => x.BrickPotential_ID)
            .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.Brickworks)
            .WithMany(x => x.BrickEntityICollection)
            .IsRequired(false);
        _ = builder.Entity<Manufactory>()
            .HasMany(x => x.BrickEntityICollection)
            .WithOne(x => x.Brickworks)
            .HasForeignKey(x => x.Brickworks_ID)
            .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.CityOfBrickworks)
            .WithMany(x => x.BrickEntityICollection)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasMany(x => x.BrickEntityICollection)
            .WithOne(x => x.CityOfBrickworks)
            .HasForeignKey(x => x.CityOfBrickworks_ID)
            .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.Brickmaker)
            .WithMany(x => x.BrickEntityBrickmakerICollection)
            .IsRequired(false);
        _ = builder.Entity<Person>()
            .HasMany(x => x.BrickEntityBrickmakerICollection)
            .WithOne(x => x.Brickmaker)
            .HasForeignKey(x => x.Brickmaker_ID)
            .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.Owner)
            .WithMany(x => x.BrickEntityOwnerICollection)
            .HasForeignKey(x => x.Owner_ID)
            .IsRequired(false);
        _ = builder.Entity<Person>()
            .HasMany(x => x.BrickEntityOwnerICollection)
            .WithOne(x => x.Owner)
            .IsRequired(false);
        _ = builder.Entity<PostcardEntity>()
            .HasOne(x => x.Owner)
            .WithMany(x => x.PostcardEntityOwnerICollection)
            .HasForeignKey(x => x.Owner_ID)
            .IsRequired(false);
        _ = builder.Entity<Person>()
            .HasMany(x => x.PostcardEntityOwnerICollection)
            .WithOne(x => x.Owner)
            .IsRequired(false);
        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.ProductPictureICollection)
            .WithOne(pp => pp.BrickEntity)
            .HasForeignKey(pp => pp.BrickEntity_ID)
            .IsRequired(false);
        _ = builder.Entity<ProductPicture>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.ProductPictureICollection)
            .IsRequired(false);
        _ = builder.Entity<PostcardEntity>()
            .HasMany(x => x.ProductPictureICollection)
            .WithOne(pp => pp.PostcardEntity)
            .HasForeignKey(pp => pp.PostcardEntity_ID)
            .IsRequired(false);
        _ = builder.Entity<ProductPicture>()
            .HasOne(x => x.PostcardEntity)
            .WithMany(x => x.ProductPictureICollection)
            .IsRequired(false);
    }
}

