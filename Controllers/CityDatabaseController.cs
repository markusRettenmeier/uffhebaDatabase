using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CityDatabaseController(ICityRepository cityRepository,
                                        IGeographyRepository geographyRepository,
                                        IEraRepository eraRepository,
                                        IPostalcodeRepository postalcodeRepository,
                                        IOeconymRepository oeconymRepository,
                                        IProcessCity processCity) : Controller
    {
        public ActionResult AdministerCollectionCity(string statusMessage)
        {
            var cityQuery = (from c in cityRepository.GetCities()
                             select c).ToList();

            ViewData["StatusMessage"] = statusMessage;

            return View(cityQuery);
        }

        public ActionResult CreateCity(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            var model = new CityParameterModel
            {
                Geography = new Geography { IsGeographyNameRequired = false }
            };

            return View(model);
        }
        public IActionResult CreateCitySubmit(CityParameterModel model)
        {
            var (city, statuscode, message) = processCity.CreateCity(model);

            return RedirectToAction(nameof(CreateCity), new { statusMessage = message });
        }

        protected override void Dispose(bool disposing)
        {
            cityRepository.Dispose();
            oeconymRepository.Dispose();
            geographyRepository.Dispose();
            eraRepository.Dispose();
            postalcodeRepository.Dispose();
            base.Dispose(disposing);
        }
    }

    [Route("Api/CityDatabaseRestAPI")]
    public class CityDatabaseRestAPI( ICityRepository cityRepository,
                                        IGeographyRepository geographyRepository,
                                        IEraRepository eraRepository,
                                        IPostalcodeRepository postalcodeRepository,
                                        IOeconymRepository oeconymRepository,
                                        IProcessCity processCity) : Controller
    {

        [HttpPost("CreateCitySubmit")]
        public IActionResult CreateCitySubmit([FromBody] CityParameterModel model)
        {            
            var (city, statuscode, Message) = processCity.CreateCity(model);

            return StatusCode(statuscode, Message);
        }        

        protected override void Dispose(bool disposing)
        {
            cityRepository.Dispose();
            oeconymRepository.Dispose();
            geographyRepository.Dispose();
            eraRepository.Dispose();
            postalcodeRepository.Dispose();
            base.Dispose(disposing);
        }
    }       

    public interface ICityRepository : IDisposable
    {
        Task<City> AddCityAsync(City city);
        Task<City?> GetCityByIdAsync(int cityId);
        IEnumerable<CityParameterModel> GetCities();
        Task AddGeographyToCity(Geography geography, City city);
        Task AddPostalcodeToCity(Postalcode postalcode, City city);
    }
    public interface IOeconymRepository : IDisposable
    {
        Task<Oeconym> AddOeconymAsync(Oeconym oeconym);
        Oeconym? GetOeconymByName(string name);
        Oeconym? GetOeconymAsync(Oeconym oeconym);
    }
    public interface IPostalcodeRepository : IDisposable
    {
        Task<Postalcode> AddPostalcodeAsync(Postalcode postalcode);
        Postalcode? GetPostalcodeByNumber(string postalcodeNumber);
    }

    public class CityRepository(DbIdentityContext context) : ICityRepository
    {
        public async Task<City> AddCityAsync(City city)
        {
            context.City.Add(city);
            await context.SaveChangesAsync();
            return city;
        }

        public async Task<City?> GetCityByIdAsync(int cityId)
        {
            return await context.City.FindAsync(cityId);
        }

        public async Task AddGeographyToCity(Geography geography, City city)
        {
            city.Geography_ID = geography.Geography_ID;
            geography.CityICollection.Add(city);
            await context.SaveChangesAsync();
        }

        public async Task AddPostalcodeToCity(Postalcode postalcode, City city)
        {
            city.PostalcodeICollection.Add(postalcode);
            postalcode.CityICollection.Add(city);
            await context.SaveChangesAsync();
        }

        public IEnumerable<CityParameterModel> GetCities()
        {
            IQueryable<CityParameterModel> getCitiesQuery = 
                from c in context.City
                .Include(x => x.PostalcodeICollection)
                .Include(y => y.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                .Include(x => x.Geography)
                select new CityParameterModel
                {
                    City = c
                };
            var cityQueryResult = getCitiesQuery.ToList();

            return cityQueryResult;
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

    public class OeconymRepository(DbIdentityContext context) : IOeconymRepository, IDisposable
    {
        public async Task<Oeconym> AddOeconymAsync(Oeconym oeconym)
        {
            context.Oeconym.Add(oeconym);
            await context.SaveChangesAsync();
            return oeconym;
        }

        public Oeconym? GetOeconymAsync(Oeconym oeconym)
        {
            throw new NotImplementedException();
        }

        public Oeconym? GetOeconymByName(string name)
        {
            return context.Oeconym.FirstOrDefault(x => x.OeconymName == name);
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

    public class PostalcodeRepository(DbIdentityContext context) : IPostalcodeRepository, IDisposable
    {
        public async Task<Postalcode> AddPostalcodeAsync(Postalcode postalcode)
        {
            context.Postalcode.Add(postalcode);
            await context.SaveChangesAsync();
            return postalcode;
        }

        public Postalcode? GetPostalcodeByNumber(string postalcodeNumber)
        {
            return context.Postalcode.FirstOrDefault(p => p.PostalcodeNumber == postalcodeNumber);
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

    public interface IProcessCity
    {
        (City city, int statuscode, string message) CreateCity(CityParameterModel model);
        IEnumerable<City> GetCitiesWithPredicates(CityParameterModel model);
    }
    public interface IProcessOeconym
    {
        Oeconym CreateOeconym(Oeconym oeconym);
    }
    public interface IProcessCityNOeconym
    {
        CityNOeconym CreateCityNOeconym(City city, Oeconym oeconym, CityNOeconym cityNOeconym);
    }
    public interface IProcessPostalcode
    {
        Postalcode CreatePostalcode(string postalcodeNumber);
    }

    public class CityProcessor(IProcessGeography processGeography,
                                IProcessPostalcode processPostalcode,
                                IProcessOeconym processOeconym,
                                IProcessCityNOeconym processCityNOeconym,
                                ILogger<CityDatabaseRestAPI> logger,
                                IUnitOfWork unitOfWork) : IProcessCity
    {
        public (City city, int statuscode, string message) CreateCity(CityParameterModel model)
        {
            model.Geography.IsGeographyNameRequired = false;

            if (model.OeconymList.Count == 0)
                return (new(), 412, "Ort fehlt.");
            else if (model.PostalcodeNumberList.Count == 0)
                return (new(), 412, "PLZ fehlt");

            IEnumerable<City> cities = GetCitiesWithPredicates(model);
            City? cityExists = cities.FirstOrDefault();
            if (cityExists != null)
                return (cityExists, 302, "Eintrag existiert bereits.");

            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var newCity = unitOfWork.CityRepository.Insert(model.City);
                unitOfWork.Save();

                ConnectPostalcodeToCity(model, newCity);
                ConnectOeconymToCity(model, newCity);
                ConnectGeographyToCity(model, newCity);

                scope.Complete();

                return (newCity, 201, "Ort wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public IEnumerable<City> GetCitiesWithPredicates(CityParameterModel model)
        {
            ExpressionStarter<City> predicate = PredicateBuilder.New<City>();
            IEnumerable<City> cityExists = from c in unitOfWork.CityRepository.Get(includeProperties: "CityNOeconymICollection.Oeconym,PostalcodeICollection,Geography")
                                           select c;
            if (model.City.City_ID > 0)
                predicate = predicate.And(x => x.City_ID.Equals(model.City.City_ID));
            foreach (string? oeconym in model.OeconymList)
            {
                if (oeconym is not null && oeconym.Contains("§§"))
                {
                    string[] splittedOeconym = oeconym.Split("§§");
                    if (string.IsNullOrEmpty(splittedOeconym[0]))
                        continue;

                    predicate = predicate.And(x => x.CityNOeconymICollection.Any(o => o.Oeconym.OeconymName.Contains(splittedOeconym[0])));
                }
            }
            if (!string.IsNullOrEmpty(model.City.Byname))
                predicate = predicate.And(x => x.Byname != null && x.Byname.Equals(model.City.Byname));
            if (!string.IsNullOrEmpty(model.Geography.GeographyName))
                predicate = predicate.And(x => x.Geography != null && x.Geography.GeographyName == model.Geography.GeographyName);
            if (predicate.IsStarted == true)
                cityExists = cityExists.Where(predicate);
            return cityExists;
        }

        private void ConnectPostalcodeToCity(CityParameterModel model, City newCity)
        {
            foreach (var postalcodeNo in model.PostalcodeNumberList)
            {
                if(!string.IsNullOrEmpty(postalcodeNo))
                {
                    var postalcode = processPostalcode.CreatePostalcode(postalcodeNo);
                    unitOfWork.PostalcodeRepository.AddMemberToCollection(postalcode, c => c.CityICollection, newCity);
                    unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.PostalcodeICollection, postalcode);
                    unitOfWork.Save();
                }                
            }
        }
        private void ConnectOeconymToCity(CityParameterModel model, City newCity)
        {
            foreach (string? oeconym in model.OeconymList)
            {
                if (!string.IsNullOrEmpty(oeconym))
                {
                    string[] splittedOeconym = oeconym.Split("§§");
                    if (string.IsNullOrEmpty(splittedOeconym[0]))
                        continue;

                    var newOeconym = new Oeconym() { OeconymName = splittedOeconym[0] };
                    newOeconym = processOeconym.CreateOeconym(newOeconym);

                    bool currentName = Boolean.Parse(splittedOeconym[1]);
                    if (model.OeconymList.Count == 1 && currentName == false)
                        currentName = true;
                    var newCityNOeconym = new CityNOeconym() { CurrentName =  currentName};
                    processCityNOeconym.CreateCityNOeconym(newCity, newOeconym, newCityNOeconym);
                    
                    unitOfWork.OeconymRepository.SetForeignKey(newOeconym, n => n.Oeconym_ID, newCityNOeconym.Oeconym_ID);
                    unitOfWork.OeconymRepository.AddMemberToCollection(newOeconym, o => o.CityNOeconyms, newCityNOeconym);
                    unitOfWork.CityRepository.SetForeignKey(newCity, c => c.City_ID, newCityNOeconym.City_ID);
                    unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.CityNOeconymICollection, newCityNOeconym);
                    unitOfWork.Save();                 
                }
            }
        }

        private void ConnectGeographyToCity(CityParameterModel model, City city)
        {
            if (!string.IsNullOrEmpty(model.Geography.GeographyName))
            {
                var newGeography = processGeography.CreateGeography(model.Geography.GeographyName);
                unitOfWork.CityRepository.SetForeignKey(city, c => c.Geography_ID, newGeography.Geography_ID);
                unitOfWork.GeographyRepository.AddMemberToCollection(newGeography, l => l.CityICollection, city);
                unitOfWork.Save();
            }
        }
    }

    public class OeconymProcessor(IUnitOfWork unitOfWork) : IProcessOeconym 
    {
        public Oeconym CreateOeconym(Oeconym oeconym)
        {
            if (string.IsNullOrEmpty(oeconym.OeconymName))
            {
                throw new NullReferenceException();
            }
            Oeconym? existingOeconym = GetOeconym(oeconym);

            if (existingOeconym != null)
            {
                return existingOeconym;
            }
            else
            {
                Oeconym newOeconym = new() { OeconymName = oeconym.OeconymName };
                newOeconym = unitOfWork.OeconymRepository.Insert(newOeconym);
                unitOfWork.Save();
                return newOeconym;
            }
        }

        public Oeconym? GetOeconym(Oeconym oeconym)
        {
            return (from o in unitOfWork.OeconymRepository.Get()
                    where o.OeconymName == oeconym.OeconymName
                    select o).FirstOrDefault();
        }
    }

    public class CityNOeconymProcessor(IUnitOfWork unitOfWork) : IProcessCityNOeconym
    {
        public CityNOeconym CreateCityNOeconym(City city, Oeconym oeconym, CityNOeconym cityNOeconym)
        {
            if(city.City_ID == 0 || oeconym.Oeconym_ID == 0)
            {
                throw new NullReferenceException();
            }

            cityNOeconym.City_ID = city.City_ID;
            cityNOeconym.City = city;
            cityNOeconym.Oeconym_ID = oeconym.Oeconym_ID;
            cityNOeconym.Oeconym = oeconym;
            CityNOeconym newCityNOeconym = unitOfWork.CityNOeconymRepository.Insert(cityNOeconym);
            unitOfWork.Save();
            
            return newCityNOeconym;
        }
    }

    public class PostalcodeProcessor(IUnitOfWork unitOfWork) : IProcessPostalcode
    {
        public Postalcode CreatePostalcode(string postalcodeNumber)
        {
            if (string.IsNullOrEmpty(postalcodeNumber))
            {
                throw new NullReferenceException();
            }
            Postalcode? existingPostalcode = GetPostalcode(postalcodeNumber);

            if (existingPostalcode != null)
            {
                return existingPostalcode;
            }
            else
            {
                var newPostalcode = new Postalcode { PostalcodeNumber = postalcodeNumber };
                newPostalcode = unitOfWork.PostalcodeRepository.Insert(newPostalcode);
                unitOfWork.Save();
                return newPostalcode;
            }
        }

        public Postalcode? GetPostalcode(string postalcodeNumber)
        {
            var existingPostalcode = (from p in unitOfWork.PostalcodeRepository.Get()
                                       select p).Where(x => x.PostalcodeNumber == postalcodeNumber).FirstOrDefault();
            return existingPostalcode;
        }
    }
}
