using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CityDatabase;

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
            List<City> Cities = (from c in _dbIdentityContext.City.Include(m => m.ManufactoryList)
                            .Include(x => x.Geography)
                            .Include(x => x.CityNOeconymICollection.Where(y => y.CurrentName)).ThenInclude(x => x.Oeconym)
                                 where c.ManufactoryList.Any(c => c.Manufactory_ID.Equals(term))
                                 select c).ToList();
            return Ok(Cities);
        }

        [HttpGet("autocompleteProductionFacility")]
        public IActionResult AutoCompleteProductionFacility(string term)
        {
            List<string> productionFacilityList = (from p in _dbIdentityContext.ProductionFacility
                                                   where p.ProductionFacilityName.Contains(term)
                                                   select p.ProductionFacilityName).ToList();
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
    }
}