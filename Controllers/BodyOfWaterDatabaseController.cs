using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BodyOfWaterDatabaseController(IProcessPlace processPlace,
        IProcessBodyOfWater processBodyOfWater,
        IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, PlaceSearchParameter placeSearchParameter)
        {
            HandleStatus(status);

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.TransportRoute != null)];
            return View(placeList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);

            return View();
        }
        public IActionResult CreateSubmit(BodyOfWaterOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processBodyOfWater.Create(model);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameter { PlaceID = [id] }).FirstOrDefault();

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
            (int placeID, int _, string statusMessage) = processBodyOfWater.Edit(model);
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
