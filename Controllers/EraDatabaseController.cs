using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Data;
using Sammlerplattform.Models;

namespace Sammlerplattform.Controllers
{
    public class EraDatabaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }


    public interface IEraRepository : IDisposable
    {
        Task<Era> AddEraAsync(Era era);
        Era? GetEraByName(string name);
    }

    public class EraRepository(DbIdentityContext context) : IEraRepository
    {
        public async Task<Era> AddEraAsync(Era era)
        {
            context.Era.Add(era);
            await context.SaveChangesAsync();
            return era;
        }

        public Era? GetEraByName(string name)
        {
            return context.Era.FirstOrDefault(p => p.EraLong == name);
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

    public interface IProcessEra
    {
        Era InsertEra(string eraLong, string? eraShort = null);
    }
    public class EraProcessor(IUnitOfWork unitOfWork) : IProcessEra
    {
        public Era InsertEra(string eraLong, string? eraShort = null)
        {
            if (string.IsNullOrEmpty(eraLong))
            {
                throw new NullReferenceException();
            }
            var existingEra = (from e in unitOfWork.EraRepository.Get()
                               select e).Where(x => x.EraLong != null && x.EraLong.Equals(eraLong)).FirstOrDefault();

            if (existingEra != null)
            {
                return existingEra;
            }
            else
            {
                var newEra = new Era { EraLong = eraLong };
                if(string.IsNullOrEmpty(eraShort))
                    newEra.EraShort = eraShort;
                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();
                return newEra;
            }
        }
    }
}
