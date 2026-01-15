using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class FieldDatabaseController(IProcessPlace processPlace
        , IProcessField processField) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Field != null)];
            return View(placeList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(FieldOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int _) = processField.Insert(model);
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processField.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
