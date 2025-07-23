using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.Processes.CityProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CityDatabaseController(IProcessCity processCity) : Controller
    {
        public ActionResult AdministerCollectionCity(string statusMessage, CitySearchParameterModel citySearchParameters)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View(processCity.GetCityOPMWithPredicates(citySearchParameters));
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
            ViewData["StatusMessage"] = statusMessage;

            CitySearchParameterModel citySearch = new();
            citySearch.ParentCityID.Add(id);

            CityOperationParameterModel citySelect = processCity.GetCityOPMWithPredicates(citySearch).First();
            foreach (Postalcode postalcode in citySelect.City.PostalcodeList)
            {
                citySelect.PostalcodeNumberList.Add(postalcode.PostalcodeNumber);
            }
            foreach (CityNOeconym cno in citySelect.City.CityNOeconymList)
            {
                citySelect.OeconymList.Add(cno.Oeconym.OeconymName + "§§" + cno.CurrentName);
            }

            return View(citySelect);
        }

        public IActionResult EditCitySubmit(CityOperationParameterModel model)
        {
            (City city, int _, string statusMessage) = processCity.EditCity(model);

            return RedirectToAction(nameof(EditCity), new { statusMessage, id = city.CityID });
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
}
