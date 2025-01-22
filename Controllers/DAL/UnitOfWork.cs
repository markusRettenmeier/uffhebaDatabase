using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;

namespace Sammlerplattform.Controllers.DAL
{
    public class UnitOfWork(DbIdentityContext context) : IDisposable, IUnitOfWork
    {
        private GenericRepository<City>? cityRepository;
        private GenericRepository<Oeconym>? oeconymRepository;
        private GenericRepository<Postalcode>? postalcodeRepository;
        private GenericRepository<Era>? eraRepository;
        private GenericRepository<Geography>? geographyRepository;
        private GenericRepository<CityNOeconym>? cityNOeconymRepository;
        private GenericRepository<Manufactory>? manufactoryRepository;
        private GenericRepository<ProductionFacility>? productionFacilityRepository;
        private GenericRepository<BrickPotential>? brickPotentialRepository;
        private GenericRepository<BrickEntity>? brickEntityRepository;
        private GenericRepository<Brickname>? bricknameRepository;
        private GenericRepository<Person>? personRepository;
        private GenericRepository<Prize>? prizeRepository;
        private GenericRepository<Profession>? professionRepository;
        private GenericRepository<ProductPicture>? productPictureRepository;

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
        public GenericRepository<CityNOeconym> CityNOeconymRepository
        {
            get
            {
                cityNOeconymRepository ??= new GenericRepository<CityNOeconym>(context);
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
        public GenericRepository<Profession> ProfessionRepository
        {
            get
            {
                professionRepository ??= new GenericRepository<Profession>(context);
                return professionRepository;
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

    public interface IUnitOfWork : IDisposable
    {
        GenericRepository<City> CityRepository { get; }
        GenericRepository<Oeconym> OeconymRepository { get; }
        GenericRepository<Postalcode> PostalcodeRepository { get; }
        GenericRepository<Era> EraRepository { get; }
        GenericRepository<Geography> GeographyRepository { get; }
        GenericRepository<CityNOeconym> CityNOeconymRepository { get; }
        GenericRepository<Manufactory> ManufactoryRepository { get; }
        GenericRepository<ProductionFacility> ProductionFacilityRepository { get; }
        GenericRepository<BrickPotential> BrickPotentialRepository { get; }
        GenericRepository<BrickEntity> BrickEntityRepository { get; }
        GenericRepository<Brickname> BricknameRepository { get; }
        GenericRepository<Person> PersonRepository { get; }
        GenericRepository<Prize> PrizeRepository { get; }
        GenericRepository<Profession> ProfessionRepository { get; }
        GenericRepository<ProductPicture> ProductPictureRepository { get; }
        void Save();
    }
}
