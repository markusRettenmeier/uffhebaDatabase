using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;

//using Sammlerplattform.Models.CollectionItemDatabase.ObjectLayerDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
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
using Sammlerplattform.Models.ProcessOfManufactureDatabase;

namespace Sammlerplattform.Data
{
    public interface IUnitOfWork : IDisposable
    {
        GenericRepository<Postalcode> PostalcodeRepository { get; }
        GenericRepository<Era> EraRepository { get; }
        GenericRepository<ProductionFacility> ProductionFacilityRepository { get; }
        GenericRepository<CollectionItemPicture> CollectionItemPictureRepository { get; }
        GenericRepository<ProcessOfManufacture> ProcessOfManufactureRepository { get; }
        GenericRepository<Color> ColorRepository { get; }
        GenericRepository<CollectionItemNColor> CollectionItemNColorRepository { get; }
        GenericRepository<Material> MaterialRepository { get; }
        GenericRepository<CollectionItemNMaterial> CollectionItemNMaterialRepository { get; }
        GenericRepository<State> StateRepository { get; }
        GenericRepository<Place> PlaceRepository { get; }
        GenericRepository<Toponymy> ToponymyRepository { get; }
        GenericRepository<PlaceNToponymy> PlaceNToponomyRepository { get; }
        GenericRepository<Settlement> SettlementRepository { get; }
        GenericRepository<SettlementNPostalcode> SettlementNPostalcodeRepository { get; }
        GenericRepository<BodyOfWater> BodyOfWaterRepository { get; }
        GenericRepository<Building> BuildingRepository { get; }
        GenericRepository<Field> FieldRepository { get; }
        GenericRepository<Region> RegionRepository { get; }
        GenericRepository<Relief> ReliefRepository { get; }
        GenericRepository<TransportRoute> TransportRouteRepository { get; }
        GenericRepository<Party> PartyRepository { get; }
        GenericRepository<Individual> IndividualRepository { get; }
        GenericRepository<Organization> OrganizationRepository { get; }
        GenericRepository<CollectionItemNPlace> CollectionItemNPlaceRepository { get; }
        GenericRepository<CollectionItemNParty> CollectionItemNPartyRepository { get; }
        GenericRepository<CollectionItemEntity> CollectionItemEntityRepository { get; }
        GenericRepository<CollectionItemPotential> CollectionItemPotentialRepository { get; }
        GenericRepository<CollectionArea> CollectionAreaRepository { get; }
        GenericRepository<CollectionAttribute> CollectionAttributeRepository { get; }
        GenericRepository<CollectionItemValue> CollectionItemValueRepository { get; }
        GenericRepository<Concept> ConceptRepository { get; }
        GenericRepository<ConceptRelation> ConceptRelationRepository { get; }

        void Save();
    }

    public class UnitOfWork(DbIdentityContext context) : IDisposable, IUnitOfWork
    {
        private GenericRepository<Postalcode>? postalcodeRepository;
        private GenericRepository<Era>? eraRepository;
        private GenericRepository<ProductionFacility>? productionFacilityRepository;
        private GenericRepository<CollectionItemPicture>? collectionItemPictureRepository;
        private GenericRepository<ProcessOfManufacture>? processOfManufactureRepository;
        private GenericRepository<Color>? colorRepository;
        private GenericRepository<CollectionItemNColor>? collectionItemNColorRepository;
        private GenericRepository<Material>? materialRepository;
        private GenericRepository<CollectionItemNMaterial>? collectionItemNMaterialRepository;
        private GenericRepository<State>? stateRepository;
        private GenericRepository<Place>? placeRepository;
        private GenericRepository<Toponymy>? toponymyRepository;
        private GenericRepository<PlaceNToponymy>? placeNToponymyRepository;
        private GenericRepository<Settlement>? settlementRepository;
        private GenericRepository<SettlementNPostalcode>? settlementNPostalcodeRepository;
        private GenericRepository<BodyOfWater>? bodyOfWaterRepository;
        private GenericRepository<Building>? buildingRepository;
        private GenericRepository<Field>? fieldRepository;
        private GenericRepository<Region>? regionRepository;
        private GenericRepository<Relief>? reliefRepository;
        private GenericRepository<TransportRoute>? transportRouteRepository;
        private GenericRepository<Party>? partyRepository;
        private GenericRepository<Individual>? individualRepository;
        private GenericRepository<Organization>? organizationRepository;
        private GenericRepository<CollectionItemNPlace>? collectionItemNPlaceRepository;
        private GenericRepository<CollectionItemNParty>? collectionItemNPartyRepository;
        private GenericRepository<CollectionItemEntity>? collectionItemEntityRepository;
        private GenericRepository<CollectionItemPotential>? collectionItemPotentialRepository;
        private GenericRepository<CollectionArea>? collectionAreaRepository;
        private GenericRepository<CollectionAttribute>? collectionAttributeRepository;
        private GenericRepository<CollectionItemValue>? collectionItemValueRepository;
        private GenericRepository<Concept>? conceptRepository;
        private GenericRepository<ConceptRelation>? conceptRelationRepository;

        public GenericRepository<Postalcode> PostalcodeRepository
        {
            get
            {
                postalcodeRepository = new GenericRepository<Postalcode>(context);
                return postalcodeRepository;
            }
        }
        public GenericRepository<Era> EraRepository
        {
            get
            {
                eraRepository ??= new GenericRepository<Era>(context);
                return eraRepository;
            }
        }
        public GenericRepository<ProductionFacility> ProductionFacilityRepository
        {
            get
            {
                productionFacilityRepository ??= new GenericRepository<ProductionFacility>(context);
                return productionFacilityRepository;
            }
        }
        public GenericRepository<CollectionItemPicture> CollectionItemPictureRepository
        {
            get
            {
                collectionItemPictureRepository ??= new GenericRepository<CollectionItemPicture>(context);
                return collectionItemPictureRepository;
            }
        }
        public GenericRepository<ProcessOfManufacture> ProcessOfManufactureRepository
        {
            get
            {
                processOfManufactureRepository ??= new GenericRepository<ProcessOfManufacture>(context);
                return processOfManufactureRepository;
            }
        }
        public GenericRepository<Color> ColorRepository
        {
            get
            {
                colorRepository ??= new GenericRepository<Color>(context);
                return colorRepository;
            }
        }
        public GenericRepository<CollectionItemNColor> CollectionItemNColorRepository
        {
            get
            {
                collectionItemNColorRepository ??= new GenericRepository<CollectionItemNColor>(context);
                return collectionItemNColorRepository;
            }
        }

        public GenericRepository<Material> MaterialRepository
        {
            get
            {
                materialRepository ??= new GenericRepository<Material>(context);
                return materialRepository;

            }
        }
        public GenericRepository<CollectionItemNMaterial> CollectionItemNMaterialRepository
        {
            get
            {
                collectionItemNMaterialRepository ??= new GenericRepository<CollectionItemNMaterial>(context);
                return collectionItemNMaterialRepository;
            }
        }
        public GenericRepository<State> StateRepository
        {
            get
            {
                stateRepository ??= new GenericRepository<State>(context);
                return stateRepository;
            }
        }
        public GenericRepository<Place> PlaceRepository
        {
            get
            {
                placeRepository ??= new GenericRepository<Place>(context);
                return placeRepository;
            }
        }
        public GenericRepository<Settlement> SettlementRepository
        {
            get
            {
                settlementRepository ??= new GenericRepository<Settlement>(context);
                return settlementRepository;
            }
        }
        public GenericRepository<Toponymy> ToponymyRepository
        {
            get
            {
                toponymyRepository ??= new GenericRepository<Toponymy>(context);
                return toponymyRepository;
            }
        }
        public GenericRepository<PlaceNToponymy> PlaceNToponomyRepository
        {
            get
            {
                placeNToponymyRepository ??= new GenericRepository<PlaceNToponymy>(context);
                return placeNToponymyRepository;
            }
        }
        public GenericRepository<SettlementNPostalcode> SettlementNPostalcodeRepository
        {
            get
            {
                settlementNPostalcodeRepository ??= new GenericRepository<SettlementNPostalcode>(context);
                return settlementNPostalcodeRepository;
            }
        }
        public GenericRepository<BodyOfWater> BodyOfWaterRepository
        {
            get
            {
                bodyOfWaterRepository ??= new GenericRepository<BodyOfWater>(context);
                return bodyOfWaterRepository;
            }
        }
        public GenericRepository<Building> BuildingRepository
        {
            get
            {
                buildingRepository ??= new GenericRepository<Building>(context);
                return buildingRepository;
            }
        }
        public GenericRepository<Field> FieldRepository
        {
            get
            {
                fieldRepository ??= new GenericRepository<Field>(context);
                return fieldRepository;
            }
        }
        public GenericRepository<Region> RegionRepository
        {
            get
            {
                regionRepository ??= new GenericRepository<Region>(context);
                return regionRepository;
            }
        }
        public GenericRepository<Relief> ReliefRepository
        {
            get
            {
                reliefRepository ??= new GenericRepository<Relief>(context);
                return reliefRepository;
            }
        }
        public GenericRepository<TransportRoute> TransportRouteRepository
        {
            get
            {
                transportRouteRepository ??= new GenericRepository<TransportRoute>(context);
                return transportRouteRepository;
            }
        }
        public GenericRepository<Party> PartyRepository
        {
            get
            {
                partyRepository ??= new GenericRepository<Party>(context);
                return partyRepository;
            }
        }
        public GenericRepository<Individual> IndividualRepository
        {
            get
            {
                individualRepository ??= new GenericRepository<Individual>(context);
                return individualRepository;
            }
        }
        public GenericRepository<Organization> OrganizationRepository
        {
            get
            {
                organizationRepository ??= new GenericRepository<Organization>(context);
                return organizationRepository;
            }
        }
        public GenericRepository<CollectionItemNPlace> CollectionItemNPlaceRepository
        {
            get
            {
                collectionItemNPlaceRepository ??= new GenericRepository<CollectionItemNPlace>(context);
                return collectionItemNPlaceRepository;
            }
        }
        public GenericRepository<CollectionItemNParty> CollectionItemNPartyRepository
        {
            get
            {
                collectionItemNPartyRepository ??= new GenericRepository<CollectionItemNParty>(context);
                return collectionItemNPartyRepository;
            }
        }
        public GenericRepository<CollectionItemEntity> CollectionItemEntityRepository
        {
            get
            {
                collectionItemEntityRepository ??= new GenericRepository<CollectionItemEntity>(context);
                return collectionItemEntityRepository;
            }
        }
        public GenericRepository<CollectionItemPotential> CollectionItemPotentialRepository
        {
            get
            {
                collectionItemPotentialRepository ??= new GenericRepository<CollectionItemPotential>(context);
                return collectionItemPotentialRepository;
            }
        }
        public GenericRepository<CollectionArea> CollectionAreaRepository
        {
            get
            {
                collectionAreaRepository ??= new GenericRepository<CollectionArea>(context);
                return collectionAreaRepository;
            }
        }
        public GenericRepository<CollectionAttribute> CollectionAttributeRepository
        {
            get
            {
                collectionAttributeRepository ??= new GenericRepository<CollectionAttribute>(context);
                return collectionAttributeRepository;
            }
        }
        public GenericRepository<CollectionItemValue> CollectionItemValueRepository
        {
            get
            {
                collectionItemValueRepository ??= new GenericRepository<CollectionItemValue>(context);
                return collectionItemValueRepository;
            }
        }
        public GenericRepository<Concept> ConceptRepository
        {
            get
            {
                conceptRepository ??= new GenericRepository<Concept>(context);
                return conceptRepository;
            }
        }
        public GenericRepository<ConceptRelation> ConceptRelationRepository
        {
            get
            {
                conceptRelationRepository ??= new GenericRepository<ConceptRelation>(context);
                return conceptRelationRepository;
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
