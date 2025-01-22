using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CityDatabase;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CityDatabaseController(ICityRepository cityRepository,
                                        IProcessCity processCity) : Controller
    {
        public ActionResult AdministerCollectionCity(string statusMessage, CitySearchParameterModel citySearchParameters)
        {
            List<CityOperationParameterModel> citySelect = (from c in processCity.GetCityWithPredicates(citySearchParameters)
                                                            select new CityOperationParameterModel
                                                            {
                                                                City = c
                                                            }).ToList();

            ViewData["StatusMessage"] = statusMessage;

            return View(citySelect);
        }

        public ActionResult CreateCity(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            CityOperationParameterModel model = new()
            {
                Geography = new Geography { IsGeographyNameRequired = false }
            };

            return View(model);
        }
        public IActionResult CreateCitySubmit(CityOperationParameterModel model)
        {
            (City _, int _, string message) = processCity.CreateCity(model);

            return RedirectToAction(nameof(CreateCity), new { statusMessage = message });
        }

        public ActionResult EditCity(string statusMessage, int id)
        {
            CityOperationParameterModel citySelect = (from c in cityRepository.GetAll()
                                                      where c.City.City_ID == id
                                                      select c).First();

            foreach (Postalcode postalcode in citySelect.City.PostalcodeICollection)
            {
                citySelect.PostalcodeNumberList.Add(postalcode.PostalcodeNumber);
            }
            foreach (CityNOeconym cno in citySelect.City.CityNOeconymICollection)
            {
                citySelect.OeconymList.Add(cno.Oeconym.OeconymName + "§§" + cno.CurrentName);
            }

            ViewData["StatusMessage"] = statusMessage;

            return View(citySelect);
        }
        public IActionResult EditCitySubmit(CityOperationParameterModel model)
        {
            (City city, int _, string statusMessage) = processCity.EditCity(model);

            return RedirectToAction(nameof(EditCity), new { statusMessage, id = city.City_ID });
        }
    }

    [Authorize]
    [Route("Api/CityDatabaseRestAPI")]
    public class CityDatabaseRestAPI(IProcessCity processCity) : Controller
    {

        [HttpPost("CreateCitySubmit")]
        public IActionResult CreateCitySubmit([FromBody] CityOperationParameterModel model)
        {
            (City _, int statuscode, string Message) = processCity.CreateCity(model);

            return StatusCode(statuscode, Message);
        }
    }

    public interface ICityRepository : IDisposable
    {
        Task<City> AddCityAsync(City city);
        IEnumerable<CityOperationParameterModel> GetAll();
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
            _ = context.City.Add(city);
            _ = await context.SaveChangesAsync();
            return city;
        }

        public async Task AddGeographyToCity(Geography geography, City city)
        {
            city.Geography_ID = geography.Geography_ID;
            geography.CityICollection.Add(city);
            _ = await context.SaveChangesAsync();
        }

        public async Task AddPostalcodeToCity(Postalcode postalcode, City city)
        {
            city.PostalcodeICollection.Add(postalcode);
            postalcode.CityICollection.Add(city);
            _ = await context.SaveChangesAsync();
        }

        public IEnumerable<CityOperationParameterModel> GetAll()
        {
            IQueryable<CityOperationParameterModel> getCitiesQuery = from c in context.City
                .Include(x => x.PostalcodeICollection)
                .Include(y => y.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                .Include(x => x.Geography)
                                                                     select new CityOperationParameterModel
                                                                     {
                                                                         City = c
                                                                     };

            return getCitiesQuery;
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

    public class OeconymRepository(DbIdentityContext context) : IOeconymRepository, IDisposable
    {
        public async Task<Oeconym> AddOeconymAsync(Oeconym oeconym)
        {
            _ = context.Oeconym.Add(oeconym);
            _ = await context.SaveChangesAsync();
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

    public class PostalcodeRepository(DbIdentityContext context) : IPostalcodeRepository, IDisposable
    {
        public async Task<Postalcode> AddPostalcodeAsync(Postalcode postalcode)
        {
            _ = context.Postalcode.Add(postalcode);
            _ = await context.SaveChangesAsync();
            return postalcode;
        }

        public Postalcode? GetPostalcodeByNumber(string postalcodeNumber)
        {
            return context.Postalcode.FirstOrDefault(p => p.PostalcodeNumber == postalcodeNumber);
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

    public interface IProcessCity
    {
        (City city, int statuscode, string message) CreateCity(CityOperationParameterModel model);
        (City city, int statuscode, string message) EditCity(CityOperationParameterModel model);
        IEnumerable<City> GetCityWithPredicates(CitySearchParameterModel model);
        CitySearchParameterModel CityParametersOperationToSearch(CityOperationParameterModel model);
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

    public class CityProcessor(ICityRepository cityRepository,
                                IProcessGeography processGeography,
                                IProcessPostalcode processPostalcode,
                                IProcessOeconym processOeconym,
                                IProcessCityNOeconym processCityNOeconym,
                                ILogger<CityProcessor> logger,
                                IUnitOfWork unitOfWork) : IProcessCity
    {
        public (City city, int statuscode, string message) CreateCity(CityOperationParameterModel model)
        {
            model.Geography.IsGeographyNameRequired = false;


            if (model.OeconymList.Count == 0)
            {
                return (new(), 412, "Ortsnamen angeben.");
            }
            else if (model.PostalcodeNumberList.Count == 0)
            {
                return (new(), 412, "Mindestens 1 PLZ angeben.");
            }

            IEnumerable<City> cities = GetCityWithPredicates(CityParametersOperationToSearch(model));
            City? cityExists = cities.FirstOrDefault();
            if (cityExists != null)
            {
                return (cityExists, 302, "Eintrag existiert bereits.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                City newCity = unitOfWork.CityRepository.Insert(model.City);
                unitOfWork.Save();

                ConnectPostalcodeToCity(model.PostalcodeNumberList, newCity);
                ConnectOeconymToCity(model.OeconymList, newCity);
                ConnectGeographyToCity(model.Geography, newCity);

                scope.Complete();
                return (newCity, 201, "Ort wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public (City city, int statuscode, string message) EditCity(CityOperationParameterModel model)
        {
            model.Geography.IsGeographyNameRequired = false;

            if (model.OeconymList.Count == 0)
            {
                return (model.City, 412, "Ortsnamen angeben.");
            }
            else if (model.PostalcodeNumberList.Count == 0)
            {
                return (model.City, 412, "Mindestens 1 PLZ angeben.");
            }

            City? citySelect = (from c in cityRepository.GetAll()
                                where c.City.City_ID == model.City.City_ID
                                select c.City).FirstOrDefault();
            if (citySelect == null)
            {
                return (model.City, 302, "Eintrag existiert nicht.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                citySelect.Byname = model.City.Byname;
                unitOfWork.CityRepository.Update(citySelect);
                unitOfWork.Save();

                ChangePostalcodeOfCity(model.PostalcodeNumberList, citySelect);
                ChangeOeconymOfCity(model.OeconymList, citySelect);
                ChangeGeographyOfCity(model.Geography, citySelect);

                scope.Complete();
                return (citySelect, 201, "Ort wurde geändert.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (citySelect, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public IEnumerable<City> GetCityWithPredicates(CitySearchParameterModel model)
        {
            ExpressionStarter<City> predicate = PredicateBuilder.New<City>();
            IEnumerable<City> cityExists = from c in unitOfWork.CityRepository.Get(includeProperties: "CityNOeconymICollection.Oeconym,PostalcodeICollection,Geography,ParentCity.CityNOeconymICollection")
                                           select c;
            foreach (int city_id in model.SearchCity_ID)
            {
                if (city_id > 0)
                {
                    predicate = predicate.And(x => x.City_ID.Equals(city_id));
                }
            }
            foreach (string oeconym in model.SearchOeconym)
            {
                string[] splittedString = oeconym.Split("§§");
                if (!string.IsNullOrEmpty(splittedString[0]))
                {
                    predicate = predicate.And(x => x.CityNOeconymICollection.Any(o => o.Oeconym.OeconymName.Contains(splittedString[0])));
                }
            }
            foreach (string postalcoode in model.SearchPostalcode)
            {
                if (!string.IsNullOrEmpty(postalcoode))
                {
                    predicate = predicate.And(x => x.PostalcodeICollection.Any(p => p.PostalcodeNumber.Equals(postalcoode)));
                }
            }
            foreach (string byname in model.SearchByname)
            {
                if (!string.IsNullOrEmpty(byname))
                {
                    predicate = predicate.And(x => x.Byname != null && x.Byname.Equals(byname));
                }
            }
            foreach (string geography in model.SearchGeography)
            {
                if (!string.IsNullOrEmpty(geography))
                {
                    predicate = predicate.And(x => x.Geography != null && x.Geography.GeographyName == geography);
                }
            }
            foreach (string parentcity in model.SearchParentCity)
            {
                if (!string.IsNullOrEmpty(parentcity))
                {
                    predicate = predicate.And(x => x.ParentCity != null && x.ParentCity.CityNOeconymICollection.Any(x => x.Oeconym.OeconymName == parentcity));
                }
            }
            foreach (int parentCity_Id in model.SearchParentCity_ID)
            {
                if (parentCity_Id > 0)
                {
                    predicate = predicate.And(x => x.ParentCity_ID.Equals(parentCity_Id));
                }
            }
            if (predicate.IsStarted == true)
            {
                cityExists = cityExists.Where(predicate);
            }

            return cityExists;
        }

        public CitySearchParameterModel CityParametersOperationToSearch(CityOperationParameterModel model)
        {
            CitySearchParameterModel citySearchParameterModel = new();
            citySearchParameterModel.SearchCity_ID.Add(model.City.City_ID);
            citySearchParameterModel.SearchOeconym = model.OeconymList;
            citySearchParameterModel.SearchPostalcode = model.PostalcodeNumberList;
            if (model.City.Byname != null)
            {
                citySearchParameterModel.SearchByname.Add(model.City.Byname);
            }
            if (model.Geography.GeographyName != null)
            {
                citySearchParameterModel.SearchGeography.Add(model.Geography.GeographyName);
            }
            if (model.City.ParentCity_ID != null)
            {
                citySearchParameterModel.SearchParentCity_ID.Add((int)model.City.ParentCity_ID);
            }

            return citySearchParameterModel;
        }

        private void ConnectPostalcodeToCity(List<string> postalcodeNumberList, City newCity)
        {
            foreach (string postalcodeNo in postalcodeNumberList)
            {
                if (!string.IsNullOrEmpty(postalcodeNo))
                {
                    Postalcode postalcode = processPostalcode.CreatePostalcode(postalcodeNo);
                    unitOfWork.PostalcodeRepository.AddMemberToCollection(postalcode, c => c.CityICollection, newCity);
                    unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.PostalcodeICollection, postalcode);
                    unitOfWork.Save();
                }
            }
        }
        private void ChangePostalcodeOfCity(List<string> postalcodeList, City city)
        {
            RemovePostalcodeFromCity(city);
            ConnectPostalcodeToCity(postalcodeList, city);
        }
        private void RemovePostalcodeFromCity(City city)
        {
            List<Postalcode> postalcodesToRemove = [.. city.PostalcodeICollection];

            foreach (Postalcode? postalcode in postalcodesToRemove)
            {
                unitOfWork.CityRepository.RemoveMemberFromCollection(city, c => c.PostalcodeICollection, postalcode);
                unitOfWork.PostalcodeRepository.RemoveMemberFromCollection(postalcode, p => p.CityICollection, city);
                unitOfWork.Save();
            }
        }

        private void ConnectOeconymToCity(List<string> oeconymList, City newCity)
        {
            foreach (string? oeconym in oeconymList)
            {
                if (!string.IsNullOrEmpty(oeconym))
                {
                    string[] splittedOeconym = oeconym.Split("§§");
                    if (string.IsNullOrEmpty(splittedOeconym[0]))
                    {
                        continue;
                    }

                    Oeconym newOeconym = new() { OeconymName = splittedOeconym[0] };
                    newOeconym = processOeconym.CreateOeconym(newOeconym);

                    bool currentName = bool.Parse(splittedOeconym[1]);
                    if (oeconymList.Count == 1 && currentName == false)
                    {
                        currentName = true;
                    }

                    CityNOeconym newCityNOeconym = new() { CurrentName = currentName };
                    _ = processCityNOeconym.CreateCityNOeconym(newCity, newOeconym, newCityNOeconym);

                    //unitOfWork.OeconymRepository.SetForeignKey(newOeconym, n => n.Oeconym_ID, newCityNOeconym.Oeconym_ID);
                    unitOfWork.OeconymRepository.AddMemberToCollection(newOeconym, o => o.CityNOeconymICollection, newCityNOeconym);
                    //unitOfWork.CityRepository.SetForeignKey(newCity, c => c.City_ID, newCityNOeconym.City_ID);
                    unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.CityNOeconymICollection, newCityNOeconym);
                    unitOfWork.Save();
                }
            }
        }
        private void ChangeOeconymOfCity(List<string> oeconymList, City city)
        {
            RemoveOeconymFromCity(city);
            ConnectOeconymToCity(oeconymList, city);
        }
        private void RemoveOeconymFromCity(City city)
        {
            List<CityNOeconym> cnoToRemove = [.. city.CityNOeconymICollection];

            foreach (CityNOeconym? cno in cnoToRemove)
            {
                unitOfWork.OeconymRepository.RemoveMemberFromCollection(cno.Oeconym, o => o.CityNOeconymICollection, cno);
                unitOfWork.CityRepository.RemoveMemberFromCollection(cno.City, c => c.CityNOeconymICollection, cno);
                unitOfWork.CityNOeconymRepository.Delete(cno);
                unitOfWork.Save();
            }
        }

        private void ConnectGeographyToCity(Geography? geography, City city)
        {
            if (!string.IsNullOrEmpty(geography?.GeographyName))
            {
                Geography newGeography = processGeography.CreateGeography(geography.GeographyName);
                unitOfWork.CityRepository.SetForeignKey(city, c => c.Geography_ID, newGeography.Geography_ID);
                unitOfWork.GeographyRepository.AddMemberToCollection(newGeography, l => l.CityICollection, city);
                unitOfWork.Save();
            }
        }
        private void ChangeGeographyOfCity(Geography? geography, City city)
        {
            RemoveGeographyFromCity(city);
            ConnectGeographyToCity(geography, city);
        }
        private void RemoveGeographyFromCity(City city)
        {
            if (city.Geography_ID != null && city.Geography != null)
            {
                unitOfWork.CityRepository.SetForeignKey(city, c => c.Geography_ID, null);
                unitOfWork.GeographyRepository.RemoveMemberFromCollection(city.Geography, g => g.CityICollection, city);
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
            if (city.City_ID == 0 || oeconym.Oeconym_ID == 0)
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
                Postalcode newPostalcode = new() { PostalcodeNumber = postalcodeNumber };
                newPostalcode = unitOfWork.PostalcodeRepository.Insert(newPostalcode);
                unitOfWork.Save();
                return newPostalcode;
            }
        }

        public Postalcode? GetPostalcode(string postalcodeNumber)
        {
            Postalcode? existingPostalcode = (from p in unitOfWork.PostalcodeRepository.Get()
                                              select p).Where(x => x.PostalcodeNumber == postalcodeNumber).FirstOrDefault();
            return existingPostalcode;
        }
    }
}
