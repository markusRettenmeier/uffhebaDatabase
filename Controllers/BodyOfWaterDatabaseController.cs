using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BodyOfWaterDatabaseController(IProcessPlace processPlace,
        IProcessBodyOfWater processBodyOfWater) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            //HandleStatus(status);

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.TransportRoute != null)];
            return View(placeList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            //HandleStatus(status);

            return View();
        }
        public IActionResult CreateSubmit(BodyOfWaterOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage, int _) = processBodyOfWater.Create(model);
            return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            //HandleStatus(status);

            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();

            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(new BodyOfWaterOperationParameterModel
                  {
                      Place = existingPlace,
                      BodyOfWater = existingPlace.BodyOfWater!,
                      PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                      ChildPlaceList = existingPlace.ChildPlaceList
                  });             
        }
        public IActionResult EditSubmit(BodyOfWaterOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processBodyOfWater.Edit(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        //private void HandleStatus(Status status)
        //{
        //    if (!string.IsNullOrEmpty(status.StatusMessage))
        //    {
        //        string statusMessage = stringLocalizer[status.StatusMessage];
        //        ViewData["StatusMessage"] = statusMessage; //Keine Direktzuweisung möglich, da ViewData nur Objekte annimmt
        //        ViewData["StatusCode"] = status.StatusCode;
        //    }
        //}
    }
}
