using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Data;
using Sammlerplattform.Models;

namespace Sammlerplattform.Controllers
{
    public class GeographyDatabaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }


    public interface IProcessGeography
    {
        Geography CreateGeography(string geographyName);
    }

    public class GeographyProcessor(IUnitOfWork unitOfWork) : IProcessGeography
    {
        public Geography CreateGeography(string geographyName)
        {
            if (string.IsNullOrEmpty(geographyName))
            {
                throw new NullReferenceException();
            }

            var existingGeography = (from l in unitOfWork.GeographyRepository.Get()
                                      select l).Where(x => x.GeographyName != null && x.GeographyName.Equals(geographyName)).FirstOrDefault();

            if (existingGeography != null)
            {
                return existingGeography;
            }
            else
            {
                Geography newGeography = new() { GeographyName = geographyName };
                newGeography = unitOfWork.GeographyRepository.Insert(newGeography); 
                unitOfWork.Save();
                return newGeography;
            }
        }
    }

    public interface IGeographyRepository : IDisposable
    {
        Task<Geography> AddGeographyAsync(Geography geography);
        Geography? GetGeographyByName(string geographyName);
    }

    public class GeographyRepository(DbIdentityContext context) : IGeographyRepository, IDisposable
    {
        public async Task<Geography> AddGeographyAsync(Geography geography)
        {
            context.Geography.Add(geography);
            await context.SaveChangesAsync();
            return geography;
        }

        public Geography? GetGeographyByName(string geographyName)
        {
            return context.Geography.FirstOrDefault(l => l.GeographyName == geographyName);
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
}
