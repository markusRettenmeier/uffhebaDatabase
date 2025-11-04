using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BodyOfWaterDatabaseController(IProcessPlace processPlace,
        IProcessBodyOfWater processBodyOfWater) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.TransportRoute != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(BodyOfWaterOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processBodyOfWater.CreateBodyOfWater(model);
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
                return RedirectToAction(nameof(Index), new { statusMessage = "Gewässer nicht gefunden" });
            }

            BodyOfWaterOperationParameterModel bodyOfWaterOperationParameterModel = new()
            {
                Place = existingPlace,
                BodyOfWater = existingPlace.BodyOfWater!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(bodyOfWaterOperationParameterModel);
        }
        public IActionResult EditSubmit(BodyOfWaterOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processBodyOfWater.EditBodyOfWater(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
