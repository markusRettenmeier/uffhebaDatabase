using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BuildingDatabaseController(IProcessPlace processPlace, IProcessBuilding processBuilding,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, PlaceSearchParameter placeSearchParameter)
        {
            HandleStatus(status);

            List<Place> placeList = [.. processPlace
                .GetListWithPredicate(placeSearchParameter).Where(x => x.Building != null)];
            return View(placeList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);

            return View();
        }
        public IActionResult CreateSubmit(BuildingOperationParameterModel operationParameterModel)
        {
            (int _, int statusCode, string statusMessage) = processBuilding.CreateBuilding(operationParameterModel);
                return RedirectToAction(nameof(Index), new { statusMessage });                    
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameter { PlaceID = [id] }).FirstOrDefault();

            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(new BuildingOperationParameterModel
                {
                    Place = existingPlace,
                    Building = existingPlace.Building!,
                    PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                    ChildPlaceList = existingPlace.ChildPlaceList
                });
        }
        public IActionResult EditSubmit(BuildingOperationParameterModel model)
        {
            (int placeID, int statusCode, string statusMessage) = processBuilding.EditBuilding(model);
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
