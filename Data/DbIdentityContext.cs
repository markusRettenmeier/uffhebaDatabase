using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Models.UserSettings;

namespace Sammlerplattform.Data;

public class DbIdentityContext(DbContextOptions<DbIdentityContext> options) : IdentityDbContext<UsingIdentityUser>(options)
{
    public DbSet<ConceptRelationshipQueryResult> ConceptRelationshipQueryResult { get; set; }
    public DbSet<EntityTranslation> EntityTranslation { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("DbIdentityContextConnection");
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemPictureList)
            .WithOne(pp => pp.CollectionItemEntity)
            .HasForeignKey(pp => pp.CollectionItemEntityID);
        _ = builder.Entity<CollectionItemPicture>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemPictureList);

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNColorList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<Color>()
            .HasMany(c => c.CollectionItemNColorList)
            .WithOne(x => x.Color)
            .HasForeignKey(x => x.ColorID);
        _ = builder.Entity<CollectionItemNColor>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNColorList)
            .IsRequired(true);
        _ = builder.Entity<CollectionItemNColor>()
            .HasOne(x => x.Color)
            .WithMany(x => x.CollectionItemNColorList)
            .IsRequired(true);

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNMaterialList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<Material>()
            .HasMany(c => c.CollectionItemNMaterialList)
            .WithOne(x => x.Material)
            .HasForeignKey(x => x.MaterialID);
        _ = builder.Entity<CollectionItemNMaterial>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNMaterialList)
            .IsRequired(true);
        _ = builder.Entity<CollectionItemNMaterial>()
            .HasOne(x => x.Material)
            .WithMany(x => x.CollectionItemNMaterialList)
            .IsRequired(true);

        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(x => x.State)
            .WithMany(x => x.CollectionItemEntityList);
        _ = builder.Entity<State>()
            .HasMany(x => x.CollectionItemEntityList)
            .WithOne(x => x.State)
            .HasForeignKey(x => x.StateID);
        _ = builder.Entity<State>()
            .HasOne(x => x.CollectionArea)
            .WithMany(x => x.StateList)
            .HasForeignKey(x => x.CollectionAreaID);

        _ = builder.Entity<CollectionItemEntity>()
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
            .HasOne(e => e.RelatedGeography)
            .WithOne(e => e.RelatedSettlement)
            .HasForeignKey<Settlement>(e => e.RelatedGeographyID)
            .OnDelete(DeleteBehavior.Restrict);

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

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNPartyList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<Party>()
            .HasMany(c => c.CollectionItemNPartyList)
            .WithOne(x => x.Party)
            .HasForeignKey(x => x.PartyID);
        _ = builder.Entity<CollectionItemNParty>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNPartyList);
        _ = builder.Entity<CollectionItemNParty>()
            .HasOne(x => x.Party)
            .WithMany(x => x.CollectionItemNPartyList);
        _ = builder.Entity<CollectionItemNParty>()
            .HasOne(x => x.CollectionItemPotential)
            .WithMany(x => x.CollectionItemNPartyList);
        _ = builder.Entity<CollectionItemPotential>()
            .HasMany(x => x.CollectionItemNPartyList)
            .WithOne(x => x.CollectionItemPotential)
            .HasForeignKey(x => x.CollectionItemPotentialID);

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNPlaceList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<CollectionItemPotential>()
            .HasMany(x => x.CollectionItemNPlaceList)
            .WithOne(x => x.CollectionItemPotential)
            .HasForeignKey(x => x.CollectionItemPotentialID);
        _ = builder.Entity<Place>()
            .HasMany(c => c.CollectionItemNPlaceList)
            .WithOne(x => x.Place)
            .HasForeignKey(x => x.PlaceID);
        _ = builder.Entity<CollectionItemNPlace>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNPlaceList)
            .IsRequired(true);
        _ = builder.Entity<CollectionItemNPlace>()
            .HasOne(x => x.CollectionItemPotential)
            .WithMany(x => x.CollectionItemNPlaceList)
            .IsRequired(true);
        _ = builder.Entity<CollectionItemNPlace>()
            .HasOne(x => x.Place)
            .WithMany(x => x.CollectionItemNPlaceList)
            .IsRequired(true);

        _ = builder.Entity<CollectionArea>()
            .HasMany(c => c.CollectionAttributeList)
            .WithOne(cf => cf.CollectionArea)
            .HasForeignKey(cf => cf.CollectionAreaID);
        _ = builder.Entity<CollectionAttribute>()
            .HasOne(cf => cf.CollectionArea)
            .WithMany(c => c.CollectionAttributeList)
            .IsRequired(true);
        _ = builder.Entity<CollectionArea>()
            .HasMany(cf => cf.CollectionItemEntityList)
            .WithOne(pe => pe.CollectionArea)
            .HasForeignKey(pe => pe.CollectionAreaID);
        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(pe => pe.CollectionArea)
            .WithMany(c => c.CollectionItemEntityList)
            .IsRequired(true);

        _ = builder.Entity<CollectionAttribute>()
            .HasMany(cf => cf.CollectionAttributeValueList)
            .WithOne(civ => civ.CollectionAttribute)
            .HasForeignKey(civ => civ.CollectionAttributeID);
        _ = builder.Entity<CollectionAttributeValue>()
            .HasOne(civ => civ.CollectionAttribute)
            .WithMany(cf => cf.CollectionAttributeValueList)
            .IsRequired(true);
        _ = builder.Entity<CollectionAttributeValue>()
            .HasOne(civ => civ.CollectionItemEntity)
            .WithMany(pe => pe.CollectionAttributeValueList)
            .IsRequired(false);
        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(pe => pe.CollectionAttributeValueList)
            .WithOne(civ => civ.CollectionItemEntity)
            .HasForeignKey(civ => civ.CollectionItemEntityID);
        _ = builder.Entity<CollectionAttributeValue>()
            .HasOne(civ => civ.CollectionItemPotential)
            .WithMany(pp => pp.CollectionAttributeValueList)
            .IsRequired(false);
        _ = builder.Entity<CollectionItemPotential>()
            .HasMany(pp => pp.CollectionAttributeValueList)
            .WithOne(civ => civ.CollectionItemPotential)
            .HasForeignKey(civ => civ.CollectionItemPotentialID);

        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(cie => cie.UsingIdentityUser)
            .WithMany(uiu => uiu.CollectionItemEntityList)
            .IsRequired(true);
        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(uiu => uiu.CollectionItemEntityList)
            .WithOne(cie => cie.UsingIdentityUser)
            .HasForeignKey(cie => cie.UsingIdentityUsersID);

        _ = builder.Entity<Concept>()
            .HasOne(c => c.CollectionArea)
            .WithMany(ca => ca.ConceptList)
            .IsRequired(true);
        _ = builder.Entity<CollectionArea>()
            .HasMany(ca => ca.ConceptList)
            .WithOne(c => c.CollectionArea)
            .HasForeignKey(c => c.CollectionAreaID);
        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(cip => cip.Concept)
            .WithMany(c => c.CollectionItemEntityList)
            .HasForeignKey(cip => cip.ConceptID)
            .IsRequired(false);
        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(cip => cip.Era)
            .WithMany(c => c.CollectionItemEntityList)
            .HasForeignKey(cip => cip.EraID)
            .IsRequired(false);
        _ = builder.Entity<ConceptRelationshipQueryResult>()
            .HasNoKey()                // wichtig, sonst will EF einen PK erzwingen
            .ToView(null);             // verhindert Mapping auf View/Tabelle

        _ = builder.Entity<ConceptRelation>().ToTable("ConceptRelation", b => b.IsTemporal(false)) // echte Edge-Table
           .HasNoKey()                // wichtig, sonst will EF einen PK erzwingen
           .ToView(null);             // verhindert Mapping auf View/Tabelle

        //_ = builder.Entity<ObjectLayer>()
        //    .HasOne(ol => ol.CollectionItemEntity)
        //    .WithOne(cie => cie.ObjectLayer)
        //    .HasForeignKey(ol => ol.CollectionItemEntityID)
        //    .IsRequired(true);
        _ = builder.Entity<CollectionItemEmbedding>()
            .HasOne(cie => cie.CollectionItemEntity)
            .WithOne(ce => ce.CollectionItemEmbedding)
            .IsRequired(true);
    }

//public DbSet<Sammlerplattform.Models.ProcessOfManufactureDatabase.ProcessOfManufacture> ProcessOfManufacture { get; set; } = default!;
}