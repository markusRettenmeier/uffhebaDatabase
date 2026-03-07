using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.OwnershipProofPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
//using Sammlerplattform.Models.CollectionItemDatabase.ObjectLayerDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
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
        RelationalBaseRepository<OwnershipProofPicture> OwnershipProofPictureRepository { get; }
        RelationalBaseRepository<StatePreservation> StateRepository { get; }
        RelationalBaseRepository<Place> PlaceRepository { get; }
        RelationalBaseRepository<Toponymy> ToponymyRepository { get; }
        RelationalBaseRepository<PlaceNToponymy> PlaceNToponomyRepository { get; }  
        RelationalBaseRepository<PlaceNPlace> PlaceNPlaceRepository { get; }
        RelationalBaseRepository<Party> PartyRepository { get; }
        RelationalBaseRepository<Individual> IndividualRepository { get; }
        RelationalBaseRepository<Organization> OrganizationRepository { get; }
        RelationalBaseRepository<CollectionItemNPlace> CollectionItemNPlaceRepository { get; }
        RelationalBaseRepository<CollectionItemNParty> CollectionItemNPartyRepository { get; }
        RelationalBaseRepository<CollectionItemEntity> CollectionItemEntityRepository { get; }
        RelationalBaseRepository<CollectionArea> CollectionAreaRepository { get; }
        RelationalBaseRepository<ConceptValue> ConceptValueRepository { get; }
        RelationalBaseRepository<Concept> ConceptRepository { get; }
        RelationalBaseRepository<ConceptRelationViewModel> ConceptRelationRepository { get; }
        RelationalBaseRepository<CollectionItemEmbedding> CollectionItemEmbeddingRepository { get; }
        RelationalBaseRepository<EntityTranslation> EntityTranslationRepository { get; }
        RelationalBaseRepository<CollectionSet> SetRepository { get; }
        RelationalBaseRepository<Topic> ForumTopicRepository { get; }
        RelationalBaseRepository<TopicVote> TopicVoteRepository { get; }
        RelationalBaseRepository<FidoCredential> FidoCredentialRepository { get; }

        void Save();
    }

    public class UnitOfWork(DbIdentityContext context) : IDisposable, IUnitOfWork
    {
        private RelationalBaseRepository<Era>? eraRepository;
        private RelationalBaseRepository<Industry>? industryRepository;
        private RelationalBaseRepository<CollectionItemPicture>? collectionItemPictureRepository;
        private RelationalBaseRepository<OwnershipProofPicture>? ownershipProofPictureRepository;
        private RelationalBaseRepository<StatePreservation>? stateRepository;
        private RelationalBaseRepository<Place>? placeRepository;
        private RelationalBaseRepository<Toponymy>? toponymyRepository;
        private RelationalBaseRepository<PlaceNToponymy>? placeNToponymyRepository;    
        private RelationalBaseRepository<PlaceNPlace>? placeNPlaceRepository;
        private RelationalBaseRepository<Party>? partyRepository;
        private RelationalBaseRepository<Individual>? individualRepository;
        private RelationalBaseRepository<Organization>? organizationRepository;
        private RelationalBaseRepository<CollectionItemNPlace>? collectionItemNPlaceRepository;
        private RelationalBaseRepository<CollectionItemNParty>? collectionItemNPartyRepository;
        private RelationalBaseRepository<CollectionItemEntity>? collectionItemEntityRepository;
        private RelationalBaseRepository<CollectionArea>? collectionAreaRepository;
        private RelationalBaseRepository<ConceptValue>? conceptValueRepository;
        private RelationalBaseRepository<Concept>? conceptRepository;
        private RelationalBaseRepository<ConceptRelationViewModel>? conceptRelationRepository;
        private RelationalBaseRepository<CollectionItemEmbedding>? collectionItemEmbeddingRepository;
        private RelationalBaseRepository<EntityTranslation>? entityTranslationRepository;
        private RelationalBaseRepository<CollectionSet>? setRepository;
        private RelationalBaseRepository<Topic>? forumTopicRepository;
        private RelationalBaseRepository<TopicVote>? topicVoteRepository;
        private RelationalBaseRepository<FidoCredential>? fidoCredentialRepository;

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
        public RelationalBaseRepository<OwnershipProofPicture> OwnershipProofPictureRepository
        {
            get
            {
                ownershipProofPictureRepository ??= new RelationalBaseRepository<OwnershipProofPicture>(context);
                return ownershipProofPictureRepository;
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
        public RelationalBaseRepository<Party> PartyRepository
        {
            get
            {
                partyRepository ??= new RelationalBaseRepository<Party>(context);
                return partyRepository;
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
        public RelationalBaseRepository<CollectionItemNPlace> CollectionItemNPlaceRepository
        {
            get
            {
                collectionItemNPlaceRepository ??= new RelationalBaseRepository<CollectionItemNPlace>(context);
                return collectionItemNPlaceRepository;
            }
        }
        public RelationalBaseRepository<CollectionItemNParty> CollectionItemNPartyRepository
        {
            get
            {
                collectionItemNPartyRepository ??= new RelationalBaseRepository<CollectionItemNParty>(context);
                return collectionItemNPartyRepository;
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

        public RelationalBaseRepository<CollectionSet> SetRepository
        {
            get
            {
                setRepository ??= new RelationalBaseRepository<CollectionSet>(context);
                return setRepository;
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
