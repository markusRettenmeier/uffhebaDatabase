using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    [Route("api/collections")]
    public class RestController(DbIdentityContext context, IProcessCity processCity) : Controller
    {
        private readonly DbIdentityContext _dbIdentityContext = context;

        [HttpGet("autocompleteOeconym")]
        public IActionResult AutoCompleteCity(string term)
        {
            List<string> Citys = [.. (from t in _dbIdentityContext.Oeconym where t.OeconymName.Contains(term) select t.OeconymName)];
            return Ok(Citys.Distinct());
        }
        [HttpGet("autocompleteCityID")]
        public IActionResult AutoCompleteCityID(string term)
        {
            List<int> Citys = [.. (from t in _dbIdentityContext.City.Include(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym) where t.CityNOeconymICollection.Any(x => x.Oeconym.OeconymName.Equals(term)) select t.City_ID)];
            return Ok(Citys);
        }
        [HttpGet("autocompleteGeographyName")]
        public IActionResult AutoCompleteGeographyName(string term)
        {
            List<string> selectGeographyName = [.. (from l in _dbIdentityContext.Geography where l.GeographyName != null && l.GeographyName.Contains(term) select l.GeographyName)];
            return Ok(selectGeographyName.Distinct());
        }
        [HttpGet("autocompleteGeographyID")]
        public IActionResult AutoCompleteGeographyID(string term)
        {
            int selectGeographyId = (from l in _dbIdentityContext.Geography where l.GeographyName != null && l.GeographyName.Equals(term) select l.Geography_ID).FirstOrDefault();
            return Ok(selectGeographyId);
        }
        [HttpGet("autocompleteEra")]
        public IActionResult AutoCompleteEra(string term)
        {
            List<string> eras = [.. (from p in _dbIdentityContext.Era where p.EraLong != null && p.EraLong.Contains(term) select p.EraLong)];
            return Ok(eras);
        }
        [HttpGet("autocompleteEraID")]
        public IActionResult AutoCompleteEraID(string term)
        {
            List<int> erasID = [.. (from p in _dbIdentityContext.Era where p.EraLong != null && p.EraLong.Equals(term) select p.Era_ID)];
            return Ok(erasID);
        }

        [HttpGet("autocompleteManufactory")]
        public IActionResult AutoCompleteManufactory(string term)
        {
            List<string> Manufactorys = [.. (from p in _dbIdentityContext.Manufactory where p.ManufactoryName != null && p.ManufactoryName.Contains(term) select p.ManufactoryName)];
            return Ok(Manufactorys);
        }
        [HttpGet("autocompleteManufactoryID")]
        public IActionResult AutoCompleteManufactoryID(string term)
        {
            int ManufactoryIDs = (from p in _dbIdentityContext.Manufactory
                                  where p.ManufactoryName != null && p.ManufactoryName.Equals(term)
                                  select p.Manufactory_ID).FirstOrDefault();
            return Ok(ManufactoryIDs);
        }

        [HttpGet("autocompleteCityOfManufactory")]
        public IActionResult AutocompleteCityOfManufactory(int term)
        {
            List<City> Cities = [.. (from c in _dbIdentityContext.City.Include(m => m.ManufactoryList)
                            .Include(x => x.Geography)
                            .Include(x => x.CityNOeconymICollection.Where(y => y.CurrentName)).ThenInclude(x => x.Oeconym)
                                 where c.ManufactoryList.Any(c => c.Manufactory_ID.Equals(term))
                                 select c)];
            return Ok(Cities);
        }

        [HttpGet("autocompleteProductionFacility")]
        public IActionResult AutoCompleteProductionFacility(string term)
        {
            List<string> productionFacilityList = [.. (from p in _dbIdentityContext.ProductionFacility
                                                   where p.ProductionFacilityName.Contains(term)
                                                   select p.ProductionFacilityName)];
            return Ok(productionFacilityList);
        }
        [HttpGet("autocompleteProductionFacilityID")]
        public IActionResult AutoCompleteProductionFacilityID(string term)
        {
            int productionFacilityIDs = (from p in _dbIdentityContext.ProductionFacility
                                         where p.ProductionFacilityName.Equals(term)
                                         select p.ProductionFacility_ID).FirstOrDefault();
            return Ok(productionFacilityIDs);
        }

        [HttpGet("listCities")]
        public IActionResult ListCities(string term)
        {
            CityOperationParameterModel model = new();
            model.OeconymList.Add(term + "§§");
            List<City> CityWthItsPostalcodeAndGeography = processCity.GetCityWithPredicates(processCity.CityParametersOperationToSearch(model)).ToList();

            return Ok(CityWthItsPostalcodeAndGeography);
        }

        [HttpGet("listManufacturers")]
        public IActionResult ListManufacturers(string manufacturer, string signature, string profession)
        {
            ExpressionStarter<Person> predicate = PredicateBuilder.New<Person>();
            IQueryable<Person> manufacturerIQueryable = from p in _dbIdentityContext.Person.Include(y => y.ProfessionICollection)
                                           where p.ProfessionICollection.Any(x => x.Name == profession)
                                           select p;
            if (!string.IsNullOrEmpty(manufacturer))
            {
                predicate = predicate.And(x => x.Name == manufacturer);
            }
            if (!string.IsNullOrEmpty(signature))
            {
                predicate = predicate.And(x => x.PersonSignature != null && x.PersonSignature == signature);
            }

            if (predicate.IsStarted)
            {
                manufacturerIQueryable = manufacturerIQueryable.Where(predicate);
            }
            var manufacturerList = manufacturerIQueryable.ToList();

            return Ok(manufacturerList);
        }

        [HttpGet("listManufactorys")]
        public IActionResult ListManufactorys(string manufactory, string productionFacility, string oeconym)
        {
            ExpressionStarter<Manufactory> predicate = PredicateBuilder.New<Manufactory>();
            IQueryable<Manufactory> manufactoryIQueryable = from m in _dbIdentityContext.Manufactory.Include(x => x.CityICollection).ThenInclude(x => x.CityNOeconymICollection.Where(cno => cno.CurrentName)).ThenInclude(x => x.Oeconym)
                                                                                                   .Include(x => x.ProductionFacility)
                                                           where m.ManufactoryName.Contains(manufactory)
                                                           && m.ProductionFacility != null && m.ProductionFacility.ProductionFacilityName == productionFacility
                                                           select m;
            if (!string.IsNullOrEmpty(oeconym))
            {
                predicate = predicate.And(x => x.CityICollection.Any(c => c.CityNOeconymICollection.Any(o => o.Oeconym.Equals(oeconym))));
            }
            if (predicate.IsStarted)
            {
                manufactoryIQueryable = manufactoryIQueryable.Where(predicate);
            }
            var manufactoryList = manufactoryIQueryable.ToList();

            return Ok(manufactoryList);
        }

        [HttpGet("listBricknames")]
        public IActionResult ListBricknames(string brickname, int usageInt)
        {
            ExpressionStarter<Brickname> predicate = PredicateBuilder.New<Brickname>();
            IQueryable<Brickname> bricknameIQueryable = from b in _dbIdentityContext.Brickname.Include(x => x.BrickPotential)
            select b;
            if (!string.IsNullOrEmpty(brickname))
            {
                predicate = predicate.And(x => x.Name.Contains(brickname));
            }
            if (usageInt > 0)
            {
                predicate = predicate.And(x => x.BrickPotential != null && x.BrickPotential.UsageInt == usageInt);
            }

            if (predicate.IsStarted)
            {
                bricknameIQueryable = bricknameIQueryable.Where(predicate);
            }
            var bricknameList = bricknameIQueryable.OrderBy(x => x.Name).ToList();

            return Ok(bricknameList);
        }

        [HttpGet("autocompleteBrickname")]
        public IActionResult AutocompleteBrickname(string term)
        {
            List<string> bricknameList = [.. (from b in _dbIdentityContext.Brickname
                                          where b.Name.Contains(term)
                                          select b.Name)];
            return Ok(bricknameList);
        }
        [HttpGet("autocompleteBricknameID")]
        public IActionResult AutoCompleteBricknameID(string term)
        {
            int bricknameIDs = (from p in _dbIdentityContext.Brickname
                                         where p.Name.Equals(term)
                                         select p.Brickname_ID).FirstOrDefault();
            return Ok(bricknameIDs);
        }
    }
}