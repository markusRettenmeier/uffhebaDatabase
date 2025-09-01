using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;

namespace Sammlerplattform.Data;

public class DbIdentityContext(DbContextOptions<DbIdentityContext> options) : IdentityDbContext<UsingIdentityUser>(options)
{
    public DbSet<UserPicture> UserPicture { get; set; } = null!;
    public DbSet<Manufactory> Manufactory { get; set; } = null!;
    public DbSet<Person> Person { get; set; } = null!;
    public DbSet<Era> Era { get; set; } = null!;
    public DbSet<ProductionFacility> ProductionFacility { get; set; } = null!;
    public DbSet<Brickname> Brickname { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
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
            .HasMany(e => e.CityList)
            .WithOne(e => e.Geography)
            .HasForeignKey(e => e.GeographyID)
            .IsRequired(false);
        _ = builder.Entity<City>()
            .HasOne(e => e.Geography)
            .WithMany(e => e.CityList)
            .IsRequired(false);

        _ = builder.Entity<City>()
            .HasMany(e => e.CityOeconymList)
            .WithOne(e => e.City)
            .HasForeignKey(e => e.CityID);
        _ = builder.Entity<Oeconym>()
            .HasMany(e => e.CityOeconymList)
            .WithOne(e => e.Oeconym)
            .HasForeignKey(e => e.OeconymID);
        _ = builder.Entity<CityOeconym>()
            .HasOne(e => e.City)
            .WithMany(e => e.CityOeconymList)
            .IsRequired(true);
        _ = builder.Entity<CityOeconym>()
            .HasOne(e => e.Oeconym)
            .WithMany(e => e.CityOeconymList)
            .IsRequired(true);
        _ = builder.Entity<Era>()
            .HasMany(e => e.CityOeconymList)
            .WithOne(e => e.Era)
            .HasForeignKey(e => e.EraID);

        _ = builder.Entity<City>()
            .HasMany(e => e.CityPostalcodeList)
            .WithOne(e => e.City)
            .HasForeignKey(e => e.CityID);
        _ = builder.Entity<Postalcode>()
            .HasMany(e => e.CityPostalcodeList)
            .WithOne(e => e.Postalcode)
            .HasForeignKey(e => e.PostalcodeID);
        _ = builder.Entity<CityPostalcode>()
            .HasOne(e => e.City)
            .WithMany(e => e.CityPostalcodeList)
            .IsRequired(true);
        _ = builder.Entity<CityPostalcode>()
            .HasOne(e => e.Postalcode)
            .WithMany(e => e.CityPostalcodeList)
            .IsRequired(true);
        _ = builder.Entity<Era>()
            .HasMany(e => e.CityPostalcodeList)
            .WithOne(e => e.Era)
            .HasForeignKey(e => e.EraID);

        _ = builder.Entity<City>()
            .HasOne(c => c.ParentCity)
            .WithMany(c => c.ChildCityList)
            .HasForeignKey(c => c.ParentCityID)
            .OnDelete(DeleteBehavior.Restrict);
        _ = builder.Entity<ProductionFacility>()
            .HasMany(x => x.ManufactoryList)
            .WithOne(x => x.ProductionFacility)
            .HasForeignKey(x => x.ProductionFacility_ID);
        _ = builder.Entity<Manufactory>()
            .HasOne(x => x.ProductionFacility)
            .WithMany(x => x.ManufactoryList);
        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.UsingIdentityUser)
            .WithMany(x => x.BrickEntityList);
        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(x => x.BrickEntityList)
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
        _ = builder.Entity<Person>()
            .HasMany(e => e.PrizeList)
            .WithMany(e => e.PersonList)
            .UsingEntity(
                "PersonNPrize",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Prize)).WithMany().HasForeignKey("Prize_ID"),
                j => j.HasKey("Person_ID", "Prize_ID"));
        _ = builder.Entity<Prize>()
            .HasMany(e => e.PersonList)
            .WithMany(e => e.PrizeList)
            .UsingEntity(
                "PersonNPrize",
                l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("Person_ID"),
                r => r.HasOne(typeof(Prize)).WithMany().HasForeignKey("Prize_ID"),
                j => j.HasKey("Person_ID", "Prize_ID"));
        _ = builder.Entity<BrickPotential>()
            .HasMany(x => x.BricknameSynonymList)
            .WithOne(x => x.BrickPotential)
            .HasForeignKey(x => x.BrickPotentialID);
        _ = builder.Entity<Brickname>()
            .HasOne(x => x.BrickPotential)
            .WithMany(x => x.BricknameSynonymList);
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

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.ProductNKeywordList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<Keyword>()
            .HasMany(c => c.ProductNKeywordList)
            .WithOne(x => x.Keyword)
            .HasForeignKey(x => x.KeywordID);
        _ = builder.Entity<ProductNKeyword>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.ProductNKeywordList)
            .IsRequired(true);
        _ = builder.Entity<ProductNKeyword>()
            .HasOne(x => x.Keyword)
            .WithMany(x => x.ProductNKeywordList)
            .IsRequired(true);

        _ = builder.Entity<BrickEntity>()
            .HasMany(x => x.ProductNMaterialList)
            .WithOne(x => x.BrickEntity)
            .HasForeignKey(x => x.BrickEntityID);
        _ = builder.Entity<Material>()
            .HasMany(c => c.ProductNMaterialList)
            .WithOne(x => x.Material)
            .HasForeignKey(x => x.MaterialID);
        _ = builder.Entity<ProductNMaterial>()
            .HasOne(x => x.BrickEntity)
            .WithMany(x => x.ProductNMaterialList)
            .IsRequired(true);
        _ = builder.Entity<ProductNMaterial>()
            .HasOne(x => x.Material)
            .WithMany(x => x.ProductNMaterialList)
            .IsRequired(true);

        _ = builder.Entity<BrickEntity>()
            .HasOne(x => x.Condition)
            .WithMany(x => x.ProductEntityList);
        _ = builder.Entity<Condition>()
            .HasMany(x => x.ProductEntityList)
            .WithOne(x => x.Condition)
            .HasForeignKey(x => x.ConditionID);

        _ = builder.Entity<BrickEntity>()
            .HasIndex(i => new { i.UsingIdentityUsersID, i.PersonalIdentificationNumber })
            .IsUnique()
            .HasFilter("[PersonalIdentificationNumber] IS NOT NULL");

        _ = builder.Entity<Place>()
            .HasMany(e => e.PlaceNToponymyList)
            .WithOne(e => e.Place)
            .HasForeignKey(e => e.PlaceID);
        _ = builder.Entity<Toponymy>()
            .HasMany(e => e.PlaceNToponymyList)
            .WithOne(e => e.Toponymy)
            .HasForeignKey(e => e.ToponymyID);
        _ = builder.Entity<PlaceNToponymy>()
            .HasOne(e => e.Place)
            .WithMany(e => e.PlaceNToponymyList)
            .IsRequired(true);
        _ = builder.Entity<PlaceNToponymy>()
            .HasOne(e => e.Toponymy)
            .WithMany(e => e.PlaceNToponymyList)
            .IsRequired(true);
        //_ = builder.Entity<Era>()
        //    .HasMany(e => e.PlaceNToponymyList)
        //    .WithOne(e => e.Era)
        //    .HasForeignKey(e => e.EraID);

        _ = builder.Entity<Place>()
            .HasOne(c => c.ParentPlace)
            .WithMany(c => c.ChildPlaceList)
            .HasForeignKey(c => c.ParentPlaceID)
            .OnDelete(DeleteBehavior.Restrict);

        _ = builder.Entity<Place>()
            .HasOne(e => e.BodyOfWater)
            .WithOne(e => e.Place)
            .HasForeignKey<BodyOfWater>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.Building)
            .WithOne(e => e.Place)
            .HasForeignKey<Building>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.Field)
            .WithOne(e => e.Place)
            .HasForeignKey<Field>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.Region)
            .WithOne(e => e.Place)
            .HasForeignKey<Region>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.Relief)
            .WithOne(e => e.Place)
            .HasForeignKey<Relief>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.Settlement)
            .WithOne(e => e.Place)
            .HasForeignKey<Settlement>(e => e.PlaceID)
            .IsRequired();
        _ = builder.Entity<Place>()
            .HasOne(e => e.TransportRoute)
            .WithOne(e => e.Place)
            .HasForeignKey<TransportRoute>(e => e.PlaceID)
            .IsRequired();

        _ = builder.Entity<Settlement>()
            .HasMany(e => e.SettlementNPostalcodeList)
            .WithOne(e => e.Settlement)
            .HasForeignKey(e => e.SettlementID);
        _ = builder.Entity<Postalcode>()
            .HasMany(e => e.SettlementNPostalcodeList)
            .WithOne(e => e.Postalcode)
            .HasForeignKey(e => e.PostalcodeID);
        _ = builder.Entity<SettlementNPostalcode>()
            .HasOne(e => e.Settlement)
            .WithMany(e => e.SettlementNPostalcodeList)
            .IsRequired(true);
        _ = builder.Entity<SettlementNPostalcode>()
            .HasOne(e => e.Postalcode)
            .WithMany(e => e.SettlementNPostalcodeList)
            .IsRequired(true);

        _ = builder.Entity<Settlement>()
            .HasOne(e => e.RelatedPlace)
            .WithOne(e => e.RelatedSettlement)
            .HasForeignKey<Settlement>(e => e.RelatedPlaceID)
            .OnDelete(DeleteBehavior.Restrict);

        //_ = builder.Entity<Settlement>()
        //    .HasMany(e => e.ManufactoryList)
        //    .With

       _ = builder.Entity<Party>()
            .HasOne(p => p.Individual)
            .WithOne(i => i.Party)
            .HasForeignKey<Individual>(i => i.PartyID)
            .IsRequired();
       _ = builder.Entity<Party>()
            .HasOne(p => p.Organization)
            .WithOne(i => i.Party)
            .HasForeignKey<Organization>(i => i.PartyID)
            .IsRequired();
        _ = builder.Entity<Party>()
            .HasMany(e => e.PlaceList)
            .WithMany(e => e.PartyList)
            .UsingEntity(
                "PartyNPlace",
                l => l.HasOne(typeof(Party)).WithMany().HasForeignKey("PartyID"),
                r => r.HasOne(typeof(Place)).WithMany().HasForeignKey("PlaceID"),
                j => j.HasKey("PartyID", "PlaceID"));
        _ = builder.Entity<Place>()
            .HasMany(e => e.PartyList)
            .WithMany(e => e.PlaceList)
            .UsingEntity(
                "PartyNPlace",
                l => l.HasOne(typeof(Party)).WithMany().HasForeignKey("PartyID"),
                r => r.HasOne(typeof(Place)).WithMany().HasForeignKey("PlaceID"),
                j => j.HasKey("PartyID", "PlaceID"));
        _ = builder.Entity<Organization>()
            .HasOne(o => o.ProductionFacility)
            .WithMany(p => p.OrganizationList)
            .IsRequired();

        //_ = builder.Entity<BrickEntity>()
        //    .HasMany(x => x.ProductEntityNPartyList)
        //    .WithOne(x => x.BrickEntity)
        //    .HasForeignKey(x => x.ProductEntityID);
        //_ = builder.Entity<Party>()
        //    .HasMany(c => c.ProductNPartyList)
        //    .WithOne(x => x.Party)
        //    .HasForeignKey(x => x.PartyID);
        //_ = builder.Entity<ProductEntityNParty>()
        //    .HasOne(x => x.BrickEntity)
        //    .WithMany(x => x.ProductEntityNPartyList)
        //    .IsRequired(true);
        //_ = builder.Entity<ProductEntityNParty>()
        //    .HasOne(x => x.Party)
        //    .WithMany(x => x.ProductNPartyList)
        //    .IsRequired(true);

        //_ = builder.Entity<BrickEntity>()
        //    .HasMany(x => x.ProductEntityNPlaceList)
        //    .WithOne(x => x.BrickEntity)
        //    .HasForeignKey(x => x.ProductEntityID);
        //_ = builder.Entity<Place>()
        //    .HasMany(c => c.ProductEntityNPlaceList)
        //    .WithOne(x => x.Place)
        //    .HasForeignKey(x => x.PlaceID);
        //_ = builder.Entity<ProductEntityNPlace>()
        //    .HasOne(x => x.BrickEntity)
        //    .WithMany(x => x.ProductEntityNPlaceList)
        //    .IsRequired(true);
        //_ = builder.Entity<ProductEntityNPlace>()
        //    .HasOne(x => x.Place)
        //    .WithMany(x => x.ProductEntityNPlaceList)
        //    .IsRequired(true);
    }
}

