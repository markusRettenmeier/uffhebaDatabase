using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.Processes.CityProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CityDatabaseController(IProcessCity processCity) : Controller
    {
        public ActionResult Index(string statusMessage, CitySearchParameterModel citySearchParameters)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View(processCity.GetWithPredicates(citySearchParameters));
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public IActionResult CreateSubmit(CityOperationParameterModel model)
        {
            (City _, int _, string statusMessage) = processCity.Create(model);

            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            CitySearchParameterModel citySearch = new();
            citySearch.ParentCityID.Add(id);
            CityOperationParameterModel citySelect = processCity.GetWithPredicates(citySearch).First();

            return View(citySelect);
        }

        public IActionResult EditSubmit(CityOperationParameterModel model)
        {
            (City city, int _, string statusMessage) = processCity.Edit(model);

            return RedirectToAction(nameof(Edit), new { statusMessage, id = city.CityID });
        }
    }
}
