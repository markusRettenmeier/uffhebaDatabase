using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Models.Passkey;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Models.UserSettings;

namespace Sammlerplattform.Data;

public class DbIdentityContext(DbContextOptions<DbIdentityContext> options) : IdentityDbContext<UsingIdentityUser>(options)
{
    public DbSet<ConceptRelationViewModel> ConceptRelation { get; set; } = null!;
    public DbSet<Concept> Concept { get; set; } = null!;
    public DbSet<EntityTranslation> EntityTranslation { get; set; } = null!;
    public DbSet<FidoCredential> FidoCredential { get; set; } = null!;
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
            .HasOne(x => x.StatePreservation)
            .WithMany(x => x.CollectionItemEntityList);
        _ = builder.Entity<StatePreservation>()
            .HasMany(x => x.CollectionItemEntityList)
            .WithOne(x => x.StatePreservation)
            .HasForeignKey(x => x.StatePreservationID);
        _ = builder.Entity<StatePreservation>()
            .HasOne(x => x.CollectionArea)
            .WithMany(x => x.StatePreservationList)
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

        _ = builder.Entity<PlaceNPlace>()
            .HasOne(p => p.Place1)
            .WithMany(p => p.ConnectionsAsFirst)
            .HasForeignKey(p => p.PlaceID1)
            .OnDelete(DeleteBehavior.Restrict);
        _ = builder.Entity<PlaceNPlace>()
            .HasOne(p => p.Place2)
            .WithMany(p => p.ConnectionsAsSecond)
            .HasForeignKey(p => p.PlaceID2)
            .OnDelete(DeleteBehavior.Restrict);

        _ = builder.Entity<Participant>()
             .HasOne(p => p.Individual)
             .WithOne(i => i.Participant)
             .HasForeignKey<Individual>(i => i.ParticipantID)
             .IsRequired();
        _ = builder.Entity<Participant>()
             .HasOne(p => p.Organization)
             .WithOne(i => i.Participant)
             .HasForeignKey<Organization>(i => i.ParticipantID)
             .IsRequired();

        _ = builder.Entity<ParticipantNPlace>()
            .HasOne(e => e.Place)
            .WithMany(e => e.ParticipantNPlaceList)
            .IsRequired(true);
        _ = builder.Entity<ParticipantNPlace>()
            .HasOne(e => e.Participant)
            .WithMany(e => e.ParticipantNPlaceList)
            .IsRequired(true);

        _ = builder.Entity<ParticipantNEra>()
            .HasOne(e => e.Era)
            .WithMany(e => e.ParticipantNEraList)
            .IsRequired(true);
        _ = builder.Entity<ParticipantNEra>()
            .HasOne(e => e.Participant)
            .WithMany(e => e.ParticipantNEraList)
            .IsRequired(true);

        _ = builder.Entity<Organization>()
            .HasOne(o => o.Industry)
            .WithMany(p => p.OrganizationList)
            .IsRequired();

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNParticipantList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<Participant>()
            .HasMany(c => c.CollectionItemNParticipantList)
            .WithOne(x => x.Participant)
            .HasForeignKey(x => x.ParticipantID);
        _ = builder.Entity<CollectionItemNParticipant>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNParticipantList);
        _ = builder.Entity<CollectionItemNParticipant>()
            .HasOne(x => x.Participant)
            .WithMany(x => x.CollectionItemNParticipantList);

        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(x => x.CollectionItemNPlaceList)
            .WithOne(x => x.CollectionItemEntity)
            .HasForeignKey(x => x.CollectionItemEntityID);
        _ = builder.Entity<Place>()
            .HasMany(c => c.CollectionItemNPlaceList)
            .WithOne(x => x.Place)
            .HasForeignKey(x => x.PlaceID);
        _ = builder.Entity<CollectionItemNPlace>()
            .HasOne(x => x.CollectionItemEntity)
            .WithMany(x => x.CollectionItemNPlaceList)
            .IsRequired(true);
        _ = builder.Entity<CollectionItemNPlace>()
            .HasOne(x => x.Place)
            .WithMany(x => x.CollectionItemNPlaceList)
            .IsRequired(true);

        _ = builder.Entity<CollectionArea>()
            .HasMany(cf => cf.CollectionItemEntityList)
            .WithOne(pe => pe.CollectionArea)
            .HasForeignKey(pe => pe.CollectionAreaID);
        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(pe => pe.CollectionArea)
            .WithMany(c => c.CollectionItemEntityList)
            .IsRequired(true);

        _ = builder.Entity<ConceptValue>()
            .HasOne(civ => civ.CollectionItemEntity)
            .WithMany(pe => pe.ConceptValueList)
            .IsRequired(false);
        _ = builder.Entity<CollectionItemEntity>()
            .HasMany(pe => pe.ConceptValueList)
            .WithOne(civ => civ.CollectionItemEntity)
            .HasForeignKey(civ => civ.CollectionItemEntityID);

        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(cie => cie.UsingIdentityUser)
            .WithMany(uiu => uiu.CollectionItemEntityList)
            .IsRequired(true);
        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(uiu => uiu.CollectionItemEntityList)
            .WithOne(cie => cie.UsingIdentityUser)
            .HasForeignKey(cie => cie.UsingIdentityUsersID);

        _ = builder.Entity<CollectionItemEntity>()
            .HasOne(cip => cip.Era)
            .WithMany(c => c.CollectionItemEntityList)
            .HasForeignKey(cip => cip.EraID)
            .IsRequired(false);

        _ = builder.Entity<ConceptRelationViewModel>().ToTable("ConceptRelation", b => b.IsTemporal(false)) // echte Edge-Table
           .HasNoKey()                // wichtig, sonst will EF einen PK erzwingen
           .ToView(null);             // verhindert Mapping auf View/Tabelle
        _ = builder.Entity<CollectionItemEmbedding>()
            .HasOne(cie => cie.CollectionItemEntity)
            .WithOne(ce => ce.CollectionItemEmbedding)
            .IsRequired(true);

        _ = builder.Entity<Topic>()
            .HasMany(ft => ft.VoteList)
            .WithOne(tv => tv.Topic)
            .HasForeignKey(tv => tv.TopicId);
        _ = builder.Entity<TopicVote>()
            .HasOne(tv => tv.Topic)
            .WithMany(ft => ft.VoteList)
            .IsRequired(true);
        _ = builder.Entity<TopicVote>()
            .HasOne(tv => tv.User)
            .WithMany(u => u.TopicVoteList)
            .IsRequired(true);
        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(u => u.TopicList)
            .WithOne(t => t.Author)
            .HasForeignKey(t => t.UserId);
        //_ = builder.Entity<Topic>()
        //    .HasOne(t => t.Author)
        //    .WithMany(u => u.TopicList)
        //    .IsRequired(true);

        _ = builder.Entity<FidoCredential>()
                .HasOne(f => f.User)
                .WithMany(u => u.FidoCredentialList)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        _ = builder.Entity<PlaceEditDTO>()
            .HasNoKey();

        _ = builder.Entity<CollectionItemRelationship>()
            .HasMany(rt => rt.CollectionItemNParticipantList)
            .WithOne(cip => cip.RelationType)
            .HasForeignKey(cip => cip.RelationTypeId);
        _ = builder.Entity<CollectionItemRelationship>()
            .HasMany(rt => rt.CollectionItemNPlaceList)
            .WithOne(cip => cip.RelationType)
            .HasForeignKey(cip => cip.RelationTypeId);

        _ = builder.Entity<UsingIdentityUser>()
            .HasMany(u => u.BackupCodeList)
            .WithOne(bc => bc.User)
            .HasForeignKey(bc => bc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}