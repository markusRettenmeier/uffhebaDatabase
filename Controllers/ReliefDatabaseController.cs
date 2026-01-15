using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using static DeepL.Model.DocumentStatus;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ReliefDatabaseController(IProcessPlace processPlace, IProcessRelief processRelief) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Relief != null)];
            return View(placeList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(ReliefOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int _) = processRelief.CreateRelief(model);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Place? existingPlace = processPlace.GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processRelief.EditRelief(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
