using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class FieldDatabaseController(IProcessPlace processPlace, IProcessField processField) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Field != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(FieldOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processField.CreateField(model);
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
                return RedirectToAction(nameof(Index), new { statusMessage = "Flur nicht gefunden" });
            }

            FieldOperationParameterModel fieldOperationParameterModel = new()
            {
                Place = existingPlace,
                Field = existingPlace.Field!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(fieldOperationParameterModel);
        }
        public IActionResult EditSubmit(FieldOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processField.EditField(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
