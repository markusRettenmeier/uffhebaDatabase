using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    public class RegionDatabaseController(IProcessPlace processPlace, IProcessRegion processRegion) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Region != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(RegionOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processRegion.CreateRegion(model);
            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            PlaceSearchParameter searchParameter = new()
            {
                PlaceID = [id]
            };
            Place? existingPlace = processPlace.GetListWithPredicate(searchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                return NotFound("Gewässer nicht gefunden.");
            }

            RegionOperationParameterModel regionOperationParameterModel = new()
            {
                Place = existingPlace,
                Region = existingPlace.Region!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(regionOperationParameterModel);
        }
        public IActionResult EditSubmit(RegionOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processRegion.EditRegion(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
