using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ReliefDatabaseController(IProcessPlace processPlace, IProcessRelief processRelief) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Relief != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(ReliefOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processRelief.CreateRelief(model);
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
                return RedirectToAction(nameof(Index), new { statusMessage = "Relief nicht gefunden" });
            }

            ReliefOperationParameterModel reliefOperationParameterModel = new()
            {
                Place = existingPlace,
                Relief = existingPlace.Relief!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };
            return View(reliefOperationParameterModel);
        }
        public IActionResult EditSubmit(ReliefOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processRelief.EditRelief(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
