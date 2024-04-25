using Sammlerplattform.Data;
using Sammlerplattform.Models;

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

        public GenericRepository<City> CityRepository
        {
            get
            {
                this.cityRepository ??= new GenericRepository<City>(context);
                return cityRepository;
            }
        }
        public GenericRepository<Oeconym> OeconymRepository
        {
            get
            {
                this.oeconymRepository ??= new GenericRepository<Oeconym>(context);
                return oeconymRepository;
            }
        }
        public GenericRepository<CityNOeconym> CityNOeconymRepository
        {
            get
            {
                this.cityNOeconymRepository ??= new GenericRepository<CityNOeconym> (context);
                return cityNOeconymRepository;
            }
        }
        public GenericRepository<Postalcode> PostalcodeRepository
        {
            get
            {
                this.postalcodeRepository = new GenericRepository<Postalcode>(context);
                return postalcodeRepository;
            }
        }
        public GenericRepository<Era> EraRepository
        {
            get
            {
                this.eraRepository ??= new GenericRepository<Era>(context);
                return eraRepository;
            }
        }
        public GenericRepository<Geography> GeographyRepository
        {
            get
            {
                this.geographyRepository ??= new GenericRepository<Geography> (context);
                return geographyRepository;
            }
       }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IUnitOfWork: IDisposable
    {
        GenericRepository<City> CityRepository { get; }
        GenericRepository<Oeconym> OeconymRepository { get; }
        GenericRepository<Postalcode> PostalcodeRepository { get; }
        GenericRepository<Era> EraRepository { get; }
        GenericRepository<Geography> GeographyRepository { get; }
        GenericRepository<CityNOeconym> CityNOeconymRepository { get; }
        void Save();
    }
}
