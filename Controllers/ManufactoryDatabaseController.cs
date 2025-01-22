using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ManufactoryDatabaseController(
        IManufactoryRepository manufactoryRepository,
        IProcessManufactory processManufactory) : Controller
    {
        public IActionResult AdministerCollectionManufactory(string statusMessage, ManufactorySearchParameterModel manufactorySearchParameterModel)
        {
            List<Manufactory> manufactorySelect = (from m in processManufactory.GetManufactoryWithPredicates(manufactorySearchParameterModel)
                                                   select m).ToList();

            ViewData["StatusMessage"] = statusMessage;

            return View(manufactorySelect);
        }

        public ActionResult CreateManufactory(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }
        public IActionResult CreateManufactorySubmit(ManufactoryOperationParameterModel model)
        {
            (Manufactory _, int _, string statusMessage) = processManufactory.CreateManufactory(model);

            return RedirectToAction(nameof(CreateManufactory), new { statusMessage });
        }

        public ActionResult EditManufactory(string statusMessage, int id)
        {
            ManufactoryOperationParameterModel manufactorySelect = (from m in manufactoryRepository.GetAll()
                                                                    where m.Manufactory_ID == id
                                                                    select new ManufactoryOperationParameterModel
                                                                    {
                                                                        Manufactory = m,
                                                                        ProductionFacility = m.ProductionFacility ?? new(),
                                                                    }).First();

            ViewData["StatusMessage"] = statusMessage;
            return View(manufactorySelect);
        }
        public ActionResult EditManufactorySubmit(ManufactoryOperationParameterModel manufactoryParameterModel)
        {
            (Manufactory manufactory, int _, string statusMessage) = processManufactory.EditManufactory(manufactoryParameterModel);

            return RedirectToAction(nameof(EditManufactory), new { statusMessage, id = manufactory.Manufactory_ID });
        }
    }

    [Authorize]
    [Route("Api/ManufactoryDatabaseRestAPI")]
    public class ManufactoryDatabaseRestAPI(IProcessManufactory processManufactory) : Controller
    {

        [HttpPost("CreateManufactorySubmit")]
        public IActionResult CreateManufactorySubmit([FromBody] ManufactoryOperationParameterModel model)
        {
            (Manufactory _, int statuscode, string statusMessage) = processManufactory.CreateManufactory(model);

            return StatusCode(statuscode, statusMessage);
        }
    }

    public interface IProcessManufactory
    {
        (Manufactory manufactory, int statuscode, string statusMessage) CreateManufactory(ManufactoryOperationParameterModel manufactoryParameterModel);
        (Manufactory manufactory, int statusCode, string statusMessage) EditManufactory(ManufactoryOperationParameterModel model);
        IEnumerable<Manufactory> GetManufactoryWithPredicates(ManufactorySearchParameterModel model);
        ManufactorySearchParameterModel ManufactoryParametersOperationToSearch(ManufactoryOperationParameterModel model);
    }
    public class ManufactoryProcessor(IUnitOfWork unitOfWork, ILogger<ManufactoryProcessor> logger) : IProcessManufactory
    {
        public (Manufactory manufactory, int statuscode, string statusMessage) CreateManufactory(ManufactoryOperationParameterModel model)
        {
            if (string.IsNullOrEmpty(model.Manufactory.ManufactoryName))
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Herstellername fehlt.");
            }

            Manufactory? manufactorySelect = (from m in unitOfWork.ManufactoryRepository.Get(includeProperties: "CityList,ProductionFacility")
                                              where m.ManufactoryName == model.Manufactory.ManufactoryName
                                              select m).FirstOrDefault();

            if (manufactorySelect != null)
            {
                return (manufactorySelect, 302, "Hersteller existiert bereits.");
            }
            else
            {
                try
                {
                    using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                    Manufactory newManufactory = unitOfWork.ManufactoryRepository.Insert(model.Manufactory);
                    unitOfWork.Save();

                    ConnectCityToManufactory(model.CityIDList, newManufactory);
                    ConnectProductionFacilityToManufactory(model.ProductionFacility.ProductionFacilityName, newManufactory);

                    scope.Complete();
                    return (newManufactory, 201, "Hersteller wurde erstellt.");
                }
                catch (Exception ex)
                {
                    logger.LogError("Fehler beim Hinzufügen des Herstellers: {ex}", ex);
                    return (new() { ManufactoryName = string.Empty }, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
                }

            }
        }

        public (Manufactory manufactory, int statusCode, string statusMessage) EditManufactory(ManufactoryOperationParameterModel model)
        {
            if (model.Manufactory.Manufactory_ID == 0)
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Hersteller_ID fehlt.");
            }
            else if (string.IsNullOrEmpty(model.Manufactory.ManufactoryName))
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Herstellername fehlt.");
            }

            Manufactory? manufactorySelect = (from m in unitOfWork.ManufactoryRepository.Get(includeProperties: "CityList,ProductionFacility")
                                              where m.Manufactory_ID == model.Manufactory.Manufactory_ID
                                              select m).FirstOrDefault();

            if (manufactorySelect == null)
            {
                return (new() { ManufactoryName = string.Empty }, 302, "Hersteller existiert nicht.");
            }
            else
            {
                try
                {
                    using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                    manufactorySelect.ManufactoryName = model.Manufactory.ManufactoryName;
                    unitOfWork.ManufactoryRepository.Update(manufactorySelect);
                    unitOfWork.Save();

                    ChangeCityOfManufactory(model.CityIDList, manufactorySelect);
                    ChangeProductionFacilityOfManufactory(model.ProductionFacility.ProductionFacilityName, manufactorySelect);

                    scope.Complete();
                    return (manufactorySelect, 201, "Hersteller wurde erstellt.");
                }
                catch (Exception ex)
                {
                    logger.LogError("Fehler beim Hinzufügen des Herstellers: {ex}", ex);
                    return (new() { ManufactoryName = string.Empty }, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
                }
            }
        }

        private void ConnectCityToManufactory(List<int> cityList, Manufactory manufactory)
        {
            foreach (int city in cityList)
            {
                if (city > 0)
                {
                    City? existingCity = (from c in unitOfWork.CityRepository.Get()
                                          where c.City_ID == city
                                          select c).FirstOrDefault();
                    if (existingCity != null)
                    {
                        unitOfWork.ManufactoryRepository.AddMemberToCollection(manufactory, m => m.CityICollection, existingCity);
                        unitOfWork.CityRepository.AddMemberToCollection(existingCity, c => c.ManufactoryList, manufactory);
                        unitOfWork.Save();
                    }
                    else
                    {
                        logger.LogWarning("Eingegebener Ort {city} nicht verfügbar.", city);
                    }
                }
            }
        }
        private void ChangeCityOfManufactory(List<int> cityList, Manufactory manufactory)
        {
            RemoveCityFromManufactory(manufactory);
            ConnectCityToManufactory(cityList, manufactory);
        }
        private void RemoveCityFromManufactory(Manufactory manufactory)
        {
            List<City> citiesToRemove = new(manufactory.CityICollection);

            foreach (City city in citiesToRemove)
            {
                unitOfWork.ManufactoryRepository.RemoveMemberFromCollection(manufactory, m => m.CityICollection, city);
                unitOfWork.CityRepository.RemoveMemberFromCollection(city, c => c.ManufactoryList, manufactory);
                unitOfWork.Save();
            }
        }

        private void ConnectProductionFacilityToManufactory(string productionFacilityName, Manufactory manufactory)
        {
            if (!string.IsNullOrEmpty(productionFacilityName))
            {
                ProductionFacility? existingSector = (from s in unitOfWork.ProductionFacilityRepository.Get()
                                                      where s.ProductionFacilityName == productionFacilityName
                                                      select s).FirstOrDefault();

                if (existingSector != null)
                {
                    unitOfWork.ProductionFacilityRepository.AddMemberToCollection(existingSector, i => i.ManufactoryICollection, manufactory);
                    unitOfWork.ManufactoryRepository.SetForeignKey(manufactory, m => m.ProductionFacility_ID, existingSector.ProductionFacility_ID);
                    unitOfWork.Save();
                }
                else
                {
                    logger.LogWarning("Eingegebene Produktionsstätte {productionFacilityName} nicht vorhanden", productionFacilityName);
                }
            }
        }
        private void ChangeProductionFacilityOfManufactory(string productionFacilityName, Manufactory manufactory)
        {
            RemoveProductionFacilityFromManufactory(manufactory);
            ConnectProductionFacilityToManufactory(productionFacilityName, manufactory);
        }
        private void RemoveProductionFacilityFromManufactory(Manufactory manufactory)
        {
            if (manufactory.ProductionFacility != null)
            {
                unitOfWork.ProductionFacilityRepository.RemoveMemberFromCollection(manufactory.ProductionFacility, i => i.ManufactoryICollection, manufactory);
                unitOfWork.ManufactoryRepository.SetForeignKey(manufactory, m => m.ProductionFacility_ID, null);
                unitOfWork.Save();
            }
        }

        public IEnumerable<Manufactory> GetManufactoryWithPredicates(ManufactorySearchParameterModel model)
        {
            ExpressionStarter<Manufactory> predicate = PredicateBuilder.New<Manufactory>();
            IEnumerable<Manufactory> manufactorySelect = from m in unitOfWork.ManufactoryRepository.Get(includeProperties: "CityList,ProductionFacility")
                                                         select m;
            foreach (string manufactoryName in model.SearchManufactory)
            {
                if (!string.IsNullOrEmpty(manufactoryName))
                {
                    predicate = predicate.And(x => x.ManufactoryName.Contains(manufactoryName));
                }
            }
            foreach (string oeconym in model.SearchOeconym)
            {
                if (!string.IsNullOrEmpty(oeconym))
                {
                    predicate = predicate.And(x => x.CityICollection.Any(y => y.CityNOeconymICollection.Any(o => o.Oeconym.OeconymName.Contains(oeconym))));
                }
            }
            foreach (string productionFacility in model.SearchProductionFacility)
            {
                if (!string.IsNullOrEmpty(productionFacility))
                {
                    predicate = predicate.And(x => x.ProductionFacility != null && x.ProductionFacility.ProductionFacilityName.Equals(productionFacility));
                }
            }
            if (predicate.IsStarted)
            {
                manufactorySelect = manufactorySelect.Where(predicate);
            }

            return manufactorySelect;
        }

        public ManufactorySearchParameterModel ManufactoryParametersOperationToSearch(ManufactoryOperationParameterModel model)
        {
            throw new NotImplementedException();
        }
    }

    public interface IManufactoryRepository : IDisposable
    {
        IQueryable<Manufactory> GetAll();
        Task<Manufactory> AddManufactoryAsync(Manufactory manufactory);
    }

    public class ManufactoryRepository(DbIdentityContext context) : IManufactoryRepository
    {
        public async Task<Manufactory> AddManufactoryAsync(Manufactory manufactory)
        {
            _ = context.Manufactory.Add(manufactory);
            _ = await context.SaveChangesAsync();
            return manufactory;
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

        public IQueryable<Manufactory> GetAll()
        {
            IQueryable<Manufactory> getManufactoryQuery = from m in context.Manufactory
                                        .Include(x => x.CityICollection).ThenInclude(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                        .Include(x => x.ProductionFacility)
                                                          select m;
            return getManufactoryQuery;
        }
    }
}