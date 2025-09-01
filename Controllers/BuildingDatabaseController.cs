using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    public class BuildingDatabaseController (IProcessPlace processPlace, IProcessBuilding processBuilding) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Building != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(BuildingOperationParameterModel operationParameterModel)
        {
            (int _, int _, string statusMessage) = processBuilding.CreateBuilding(operationParameterModel);
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

            BuildingOperationParameterModel operationParameterModel = new()
            {
                Place = existingPlace,
                Building = existingPlace.Building!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(operationParameterModel);
        }
        public IActionResult EditSubmit(BuildingOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processBuilding.EditBuilding(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
