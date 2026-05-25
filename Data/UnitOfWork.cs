using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Models.Passkey;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Models.Translations;

namespace Sammlerplattform.Data
{
    public interface IUnitOfWork : IDisposable
    {
        RelationalBaseRepository<Era> EraRepository { get; }
        RelationalBaseRepository<Industry> IndustryRepository { get; }
        RelationalBaseRepository<CollectionItemPicture> CollectionItemPictureRepository { get; }
        RelationalBaseRepository<StatePreservation> StateRepository { get; }
        RelationalBaseRepository<Place> PlaceRepository { get; }
        RelationalBaseRepository<Toponymy> ToponymyRepository { get; }
        RelationalBaseRepository<PlaceNToponymy> PlaceNToponomyRepository { get; }
        RelationalBaseRepository<PlaceNPlace> PlaceNPlaceRepository { get; }
        RelationalBaseRepository<Participant> ParticipantRepository { get; }
        RelationalBaseRepository<Individual> IndividualRepository { get; }
        RelationalBaseRepository<Organization> OrganizationRepository { get; }
        RelationalBaseRepository<ParticipantNPlace> ParticipantNPlaceRepository { get; }
        RelationalBaseRepository<ParticipantNEra> ParticipantNEraRepository { get; }
        RelationalBaseRepository<CollectionItemNPlace> CollectionItemNPlaceRepository { get; }
        RelationalBaseRepository<CollectionItemNParticipant> CollectionItemNParticipantRepository { get; }
        RelationalBaseRepository<CollectionItemEntity> CollectionItemEntityRepository { get; }
        RelationalBaseRepository<CollectionArea> CollectionAreaRepository { get; }
        RelationalBaseRepository<ConceptValue> ConceptValueRepository { get; }
        RelationalBaseRepository<Concept> ConceptRepository { get; }
        RelationalBaseRepository<ConceptRelationViewModel> ConceptRelationRepository { get; }
        RelationalBaseRepository<CollectionItemEmbedding> CollectionItemEmbeddingRepository { get; }
        RelationalBaseRepository<EntityTranslation> EntityTranslationRepository { get; }
        RelationalBaseRepository<Topic> ForumTopicRepository { get; }
        RelationalBaseRepository<TopicVote> TopicVoteRepository { get; }
        RelationalBaseRepository<FidoCredential> FidoCredentialRepository { get; }
        RelationalBaseRepository<CollectionItemRelationship> CppRelationshipRepository { get; }

        void Save();
    }

    public class UnitOfWork(DbIdentityContext context) : IDisposable, IUnitOfWork
    {
        private RelationalBaseRepository<Era>? eraRepository;
        private RelationalBaseRepository<Industry>? industryRepository;
        private RelationalBaseRepository<CollectionItemPicture>? collectionItemPictureRepository;
        private RelationalBaseRepository<StatePreservation>? stateRepository;
        private RelationalBaseRepository<Place>? placeRepository;
        private RelationalBaseRepository<Toponymy>? toponymyRepository;
        private RelationalBaseRepository<PlaceNToponymy>? placeNToponymyRepository;
        private RelationalBaseRepository<PlaceNPlace>? placeNPlaceRepository;
        private RelationalBaseRepository<Participant>? participantRepository;
        private RelationalBaseRepository<Individual>? individualRepository;
        private RelationalBaseRepository<Organization>? organizationRepository;
        private RelationalBaseRepository<ParticipantNPlace>? participantNPlaceRepository;
        private RelationalBaseRepository<ParticipantNEra>? participantNEraRepository;
        private RelationalBaseRepository<CollectionItemNPlace>? collectionItemNPlaceRepository;
        private RelationalBaseRepository<CollectionItemNParticipant>? collectionItemNParticipantRepository;
        private RelationalBaseRepository<CollectionItemEntity>? collectionItemEntityRepository;
        private RelationalBaseRepository<CollectionArea>? collectionAreaRepository;
        private RelationalBaseRepository<ConceptValue>? conceptValueRepository;
        private RelationalBaseRepository<Concept>? conceptRepository;
        private RelationalBaseRepository<ConceptRelationViewModel>? conceptRelationRepository;
        private RelationalBaseRepository<CollectionItemEmbedding>? collectionItemEmbeddingRepository;
        private RelationalBaseRepository<EntityTranslation>? entityTranslationRepository;
        private RelationalBaseRepository<Topic>? forumTopicRepository;
        private RelationalBaseRepository<TopicVote>? topicVoteRepository;
        private RelationalBaseRepository<FidoCredential>? fidoCredentialRepository;
        private RelationalBaseRepository<CollectionItemRelationship>? cppRelationshipRepository;

        public RelationalBaseRepository<CollectionItemEmbedding> CollectionItemEmbeddingRepository
        {
            get
            {
                collectionItemEmbeddingRepository ??= new RelationalBaseRepository<CollectionItemEmbedding>(context);
                return collectionItemEmbeddingRepository;
            }
        }
        public RelationalBaseRepository<Era> EraRepository
        {
            get
            {
                eraRepository ??= new RelationalBaseRepository<Era>(context);
                return eraRepository;
            }
        }
        public RelationalBaseRepository<Industry> IndustryRepository
        {
            get
            {
                industryRepository ??= new RelationalBaseRepository<Industry>(context);
                return industryRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemPicture> CollectionItemPictureRepository
        {
            get
            {
                collectionItemPictureRepository ??= new RelationalBaseRepository<CollectionItemPicture>(context);
                return collectionItemPictureRepository;
            }
        }
        public RelationalBaseRepository<StatePreservation> StateRepository
        {
            get
            {
                stateRepository ??= new RelationalBaseRepository<StatePreservation>(context);
                return stateRepository;
            }
        }
        public RelationalBaseRepository<Place> PlaceRepository
        {
            get
            {
                placeRepository ??= new RelationalBaseRepository<Place>(context);
                return placeRepository;
            }
        }
        public RelationalBaseRepository<PlaceNPlace> PlaceNPlaceRepository
        {
            get
            {
                placeNPlaceRepository ??= new RelationalBaseRepository<PlaceNPlace>(context);
                return placeNPlaceRepository;
            }
        }
        public RelationalBaseRepository<Toponymy> ToponymyRepository
        {
            get
            {
                toponymyRepository ??= new RelationalBaseRepository<Toponymy>(context);
                return toponymyRepository;
            }
        }
        public RelationalBaseRepository<PlaceNToponymy> PlaceNToponomyRepository
        {
            get
            {
                placeNToponymyRepository ??= new RelationalBaseRepository<PlaceNToponymy>(context);
                return placeNToponymyRepository;
            }
        }
        public RelationalBaseRepository<Participant> ParticipantRepository
        {
            get
            {
                participantRepository ??= new RelationalBaseRepository<Participant>(context);
                return participantRepository;
            }
        }
        public RelationalBaseRepository<Individual> IndividualRepository
        {
            get
            {
                individualRepository ??= new RelationalBaseRepository<Individual>(context);
                return individualRepository;
            }
        }
        public RelationalBaseRepository<Organization> OrganizationRepository
        {
            get
            {
                organizationRepository ??= new RelationalBaseRepository<Organization>(context);
                return organizationRepository;
            }
        }
        public RelationalBaseRepository<ParticipantNPlace> ParticipantNPlaceRepository
        {
            get
            {
                participantNPlaceRepository ??= new RelationalBaseRepository<ParticipantNPlace>(context);
                return participantNPlaceRepository;
            }
        }
        public RelationalBaseRepository<ParticipantNEra> ParticipantNEraRepository
        {
            get
            {
                participantNEraRepository ??= new RelationalBaseRepository<ParticipantNEra>(context);
                return participantNEraRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemNPlace> CollectionItemNPlaceRepository
        {
            get
            {
                collectionItemNPlaceRepository ??= new RelationalBaseRepository<CollectionItemNPlace>(context);
                return collectionItemNPlaceRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemNParticipant> CollectionItemNParticipantRepository
        {
            get
            {
                collectionItemNParticipantRepository ??= new RelationalBaseRepository<CollectionItemNParticipant>(context);
                return collectionItemNParticipantRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemEntity> CollectionItemEntityRepository
        {
            get
            {
                collectionItemEntityRepository ??= new RelationalBaseRepository<CollectionItemEntity>(context);
                return collectionItemEntityRepository;
            }
        }
        public RelationalBaseRepository<CollectionArea> CollectionAreaRepository
        {
            get
            {
                collectionAreaRepository ??= new RelationalBaseRepository<CollectionArea>(context);
                return collectionAreaRepository;
            }
        }
        public RelationalBaseRepository<ConceptValue> ConceptValueRepository
        {
            get
            {
                conceptValueRepository ??= new RelationalBaseRepository<ConceptValue>(context);
                return conceptValueRepository;
            }
        }
        public RelationalBaseRepository<Concept> ConceptRepository
        {
            get
            {
                conceptRepository ??= new RelationalBaseRepository<Concept>(context);
                return conceptRepository;
            }
        }
        public RelationalBaseRepository<ConceptRelationViewModel> ConceptRelationRepository
        {
            get
            {
                conceptRelationRepository ??= new RelationalBaseRepository<ConceptRelationViewModel>(context);
                return conceptRelationRepository;
            }
        }
        public RelationalBaseRepository<EntityTranslation> EntityTranslationRepository
        {
            get
            {
                entityTranslationRepository ??= new RelationalBaseRepository<EntityTranslation>(context);
                return entityTranslationRepository;
            }
        }
        public RelationalBaseRepository<Topic> ForumTopicRepository
        {
            get
            {
                forumTopicRepository ??= new RelationalBaseRepository<Topic>(context);
                return forumTopicRepository;
            }
        }
        public RelationalBaseRepository<TopicVote> TopicVoteRepository
        {
            get
            {
                topicVoteRepository ??= new RelationalBaseRepository<TopicVote>(context);
                return topicVoteRepository;
            }
        }
        public RelationalBaseRepository<FidoCredential> FidoCredentialRepository
        {
            get
            {
                fidoCredentialRepository ??= new RelationalBaseRepository<FidoCredential>(context);
                return fidoCredentialRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemRelationship> CppRelationshipRepository
        {
            get
            {
                cppRelationshipRepository ??= new RelationalBaseRepository<CollectionItemRelationship>(context);
                return cppRelationshipRepository;
            }
        }

        public void Save()
        {
            _ = context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
