using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class FieldDatabaseController(IProcessPlace processPlace, IProcessField processField,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, PlaceSearchParameter placeSearchParameter)
        {
            HandleStatus(status);

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Field != null)];
            return View(placeList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);
            return View();
        }
        public IActionResult CreateSubmit(FieldOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processField.Create(model);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            Place? existingPlace = processPlace.GetListWithPredicate(new PlaceSearchParameter { PlaceID = [id] }).FirstOrDefault();
            if (existingPlace == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" });
            }            

            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(new FieldOperationParameterModel
                {
                    Place = existingPlace,
                    Field = existingPlace.Field!,
                    PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                    ChildPlaceList = existingPlace.ChildPlaceList
                });
        }
        public IActionResult EditSubmit(FieldOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processField.Edit(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.Message];
                ViewData["StatusCode"] = status.Code;
            }
        }
    }
}
