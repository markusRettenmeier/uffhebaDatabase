using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;

namespace Sammlerplattform.Data
{
    public interface IUnitOfWork : IDisposable
    {
        GenericRepository<City> CityRepository { get; }
        GenericRepository<Oeconym> OeconymRepository { get; }
        GenericRepository<Postalcode> PostalcodeRepository { get; }
        GenericRepository<CityPostalcode> CityPostalcodeRepository { get; }
        GenericRepository<Era> EraRepository { get; }
        GenericRepository<Geography> GeographyRepository { get; }
        GenericRepository<CityOeconym> CityNOeconymRepository { get; }
        GenericRepository<Manufactory> ManufactoryRepository { get; }
        GenericRepository<ProductionFacility> ProductionFacilityRepository { get; }
        GenericRepository<BrickPotential> BrickPotentialRepository { get; }
        GenericRepository<BrickEntity> BrickEntityRepository { get; }
        GenericRepository<Brickname> BricknameRepository { get; }
        GenericRepository<Person> PersonRepository { get; }
        GenericRepository<Prize> PrizeRepository { get; }
        GenericRepository<ProductPicture> ProductPictureRepository { get; }
        GenericRepository<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityRepository { get; }
        GenericRepository<BrickEntityNPerson> BrickEntityNPersonRepository { get; }
        GenericRepository<BrickEntityNCity> BrickEntityNCityRepository { get; }
        GenericRepository<ProcessOfManufacture> ProcessOfManufactureRepository { get; }
        GenericRepository<Color> ColorRepository { get; }
        GenericRepository<ProductNColorVariant> ProductNColorVariantRepository { get; }
        GenericRepository<Material> MaterialRepository { get; }
        GenericRepository<ProductNMaterial> ProductNMaterialRepository { get; }
        GenericRepository<Keyword> KeywordRepository { get; }
        GenericRepository<ProductNKeyword> ProductNKeywordRepository { get; }
        GenericRepository<Condition> ConditionRepository { get; }
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
        GenericRepository<ProductEntityNPlace> ProductEntityNPlaceRepository { get; }
        GenericRepository<ProductEntityNParty> ProductEntityNPartyRepository { get; }


        void Save();
    }

    public class UnitOfWork(DbIdentityContext context) : IDisposable, IUnitOfWork
    {
        private GenericRepository<City>? cityRepository;
        private GenericRepository<Oeconym>? oeconymRepository;
        private GenericRepository<Postalcode>? postalcodeRepository;
        private GenericRepository<CityPostalcode>? cityPostalcodeRepository;
        private GenericRepository<Era>? eraRepository;
        private GenericRepository<Geography>? geographyRepository;
        private GenericRepository<CityOeconym>? cityNOeconymRepository;
        private GenericRepository<Manufactory>? manufactoryRepository;
        private GenericRepository<ProductionFacility>? productionFacilityRepository;
        private GenericRepository<BrickPotential>? brickPotentialRepository;
        private GenericRepository<BrickEntity>? brickEntityRepository;
        private GenericRepository<Brickname>? bricknameRepository;
        private GenericRepository<Person>? personRepository;
        private GenericRepository<Prize>? prizeRepository;
        private GenericRepository<ProductPicture>? productPictureRepository;
        private GenericRepository<BrickEntityNManufactoryNCity>? brickEntityNManufactoryNCityRepository;
        private GenericRepository<BrickEntityNPerson>? brickEntityNPersonRepository;
        private GenericRepository<BrickEntityNCity>? brickEntityNCityRepository;
        private GenericRepository<ProcessOfManufacture>? processOfManufactureRepository;
        private GenericRepository<Color>? colorRepository;
        private GenericRepository<ProductNColorVariant>? productNColorVariantRepository;
        private GenericRepository<Material>? materialRepository;
        private GenericRepository<ProductNMaterial>? productNMaterialRepository;
        private GenericRepository<Keyword>? keywordRepository;
        private GenericRepository<ProductNKeyword>? productNKeywordRepository;
        private GenericRepository<Condition>? conditionRepository;
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
        private GenericRepository<ProductEntityNPlace>? productEntityNPlaceRepository;
        private GenericRepository<ProductEntityNParty>? productEntityNPartyRepository;


        public GenericRepository<City> CityRepository
        {
            get
            {
                cityRepository ??= new GenericRepository<City>(context);
                return cityRepository;
            }
        }
        public GenericRepository<Oeconym> OeconymRepository
        {
            get
            {
                oeconymRepository ??= new GenericRepository<Oeconym>(context);
                return oeconymRepository;
            }
        }
        public GenericRepository<CityOeconym> CityNOeconymRepository
        {
            get
            {
                cityNOeconymRepository ??= new GenericRepository<CityOeconym>(context);
                return cityNOeconymRepository;
            }
        }
        public GenericRepository<Postalcode> PostalcodeRepository
        {
            get
            {
                postalcodeRepository = new GenericRepository<Postalcode>(context);
                return postalcodeRepository;
            }
        }
        public GenericRepository<CityPostalcode> CityPostalcodeRepository
        {
            get
            {
                cityPostalcodeRepository ??= new GenericRepository<CityPostalcode>(context);
                return cityPostalcodeRepository;
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
        public GenericRepository<Geography> GeographyRepository
        {
            get
            {
                geographyRepository ??= new GenericRepository<Geography>(context);
                return geographyRepository;
            }
        }
        public GenericRepository<Manufactory> ManufactoryRepository
        {
            get
            {
                manufactoryRepository ??= new GenericRepository<Manufactory>(context);
                return manufactoryRepository;
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
        public GenericRepository<BrickPotential> BrickPotentialRepository
        {
            get
            {
                brickPotentialRepository ??= new GenericRepository<BrickPotential>(context);
                return brickPotentialRepository;
            }
        }
        public GenericRepository<BrickEntity> BrickEntityRepository
        {
            get
            {
                brickEntityRepository ??= new GenericRepository<BrickEntity>(context);
                return brickEntityRepository;
            }
        }
        public GenericRepository<Brickname> BricknameRepository
        {
            get
            {
                bricknameRepository ??= new GenericRepository<Brickname>(context);
                return bricknameRepository;
            }
        }
        public GenericRepository<Person> PersonRepository
        {
            get
            {
                personRepository ??= new GenericRepository<Person>(context);
                return personRepository;
            }
        }
        public GenericRepository<Prize> PrizeRepository
        {
            get
            {
                prizeRepository ??= new GenericRepository<Prize>(context);
                return prizeRepository;
            }
        }
        public GenericRepository<ProductPicture> ProductPictureRepository
        {
            get
            {
                productPictureRepository ??= new GenericRepository<ProductPicture>(context);
                return productPictureRepository;
            }
        }
        public GenericRepository<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityRepository
        {
            get
            {
                brickEntityNManufactoryNCityRepository ??= new GenericRepository<BrickEntityNManufactoryNCity>(context);
                return brickEntityNManufactoryNCityRepository;
            }
        }
        public GenericRepository<BrickEntityNPerson> BrickEntityNPersonRepository
        {
            get
            {
                brickEntityNPersonRepository ??= new GenericRepository<BrickEntityNPerson>(context);
                return brickEntityNPersonRepository;
            }
        }
        public GenericRepository<BrickEntityNCity> BrickEntityNCityRepository
        {
            get
            {
                brickEntityNCityRepository ??= new GenericRepository<BrickEntityNCity>(context);
                return brickEntityNCityRepository;
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
        public GenericRepository<ProductNColorVariant> ProductNColorVariantRepository
        {
            get
            {
                productNColorVariantRepository ??= new GenericRepository<ProductNColorVariant>(context);
                return productNColorVariantRepository;
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

        public GenericRepository<ProductNMaterial> ProductNMaterialRepository
        {
                       get
            {
                productNMaterialRepository ??= new GenericRepository<ProductNMaterial>(context);
                return productNMaterialRepository;
            }
        }

        public GenericRepository<Keyword> KeywordRepository
        {
            get
            {
                keywordRepository ??= new GenericRepository<Keyword>(context);
                return keywordRepository;
            }
        }

        public GenericRepository<ProductNKeyword> ProductNKeywordRepository
            {
            get
            {
                productNKeywordRepository ??= new GenericRepository<ProductNKeyword>(context);
                return productNKeywordRepository;
            }
        }

        public GenericRepository<Condition> ConditionRepository
        {
            get
            {
                conditionRepository ??= new GenericRepository<Condition>(context);
                return conditionRepository;
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
        public GenericRepository<ProductEntityNPlace> ProductEntityNPlaceRepository
        {
            get
            {
                productEntityNPlaceRepository ??= new GenericRepository<ProductEntityNPlace>(context);
                return productEntityNPlaceRepository;
            }
        }
        public GenericRepository<ProductEntityNParty> ProductEntityNPartyRepository
        {
            get
            {
                productEntityNPartyRepository ??= new GenericRepository<ProductEntityNParty>(context);
                return productEntityNPartyRepository;
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
