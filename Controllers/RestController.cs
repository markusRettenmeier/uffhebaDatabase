using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet("autocompleteAuthorArtist")]
        public IActionResult AutoCompleteAuthorArtist(string term)
        {
            List<string> aa = [.. (from p in _dbIdentityContext.AuthorArtist where p.AAName != null && p.AAName.Contains(term) select p.AAName)];
            return Ok(aa);
        }
        [HttpGet("autocompleteAuthorArtistID")]
        public IActionResult AutoCompleteAuthorArtistID(string term)
        {
            List<int> aaID = [.. (from p in _dbIdentityContext.AuthorArtist where p.AAName != null && p.AAName.Equals(term) select p.AuthorArtist_ID)];
            return Ok(aaID);
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

        [HttpGet("autocompleteManufacturer")]
        public IActionResult AutoCompleteManufacturer(string term)
        {
            List<string> Manufacturers = [.. (from p in _dbIdentityContext.Manufacturer where p.ManufacturerName != null && p.ManufacturerName.Contains(term) select p.ManufacturerName)];
            return Ok(Manufacturers);
        }
        [HttpGet("autocompleteManufacturerID")]
        public IActionResult AutoCompleteManufacturerID(string term)
        {
            int ManufacturerIDs = (from p in _dbIdentityContext.Manufacturer
                                   where p.ManufacturerName != null && p.ManufacturerName.Equals(term)
                                   select p.Manufacturer_ID).FirstOrDefault();
            return Ok(ManufacturerIDs);
        }
        [HttpGet("autocompleteCityOfManufacturer")]
        public IActionResult AutocompleteCityOfManufacturer(int term)
        {
            var Cities = (from c in _dbIdentityContext.City.Include(m => m.ManufacturerList)
                            .Include(x => x.Geography)
                            .Include(x => x.CityNOeconymICollection.Where(y => y.CurrentName)).ThenInclude(x => x.Oeconym)                      
                          where c.ManufacturerList.Any(c => c.Manufacturer_ID.Equals(term))
                          select c).ToList();
            return Ok(Cities);
        }

        [HttpGet("listCities")]
        public IActionResult ListCities(string term)
        {
            CityParameterModel model = new();
            model.OeconymList.Add(term + "§§");
            var CityWthItsPostalcodeAndGeography = processCity.GetCitiesWithPredicates(model).ToList();

            return Ok(CityWthItsPostalcodeAndGeography);
        }
    }
}