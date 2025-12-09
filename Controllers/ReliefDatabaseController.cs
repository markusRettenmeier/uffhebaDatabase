using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ReliefDatabaseController(IProcessPlace processPlace, IProcessRelief processRelief,
        IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, PlaceSearchParameter placeSearchParameter)
        {
            HandleStatus(status);

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Relief != null)];
            return View(placeList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);
            return View();
        }
        public IActionResult CreateSubmit(ReliefOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processRelief.CreateRelief(model);
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

            ReliefOperationParameterModel reliefOperationParameterModel = new()
            {
                Place = existingPlace,
                Relief = existingPlace.Relief!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };
            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(new ReliefOperationParameterModel
                {
                    Place = existingPlace,
                    Relief = existingPlace.Relief!,
                    PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                    ChildPlaceList = existingPlace.ChildPlaceList
                });
        }
        public IActionResult EditSubmit(ReliefOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processRelief.EditRelief(model);
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
