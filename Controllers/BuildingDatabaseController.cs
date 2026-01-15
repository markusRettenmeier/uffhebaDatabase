using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BuildingDatabaseController(IProcessPlace processPlace, IProcessBuilding processBuilding) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            List<Place> placeList = [.. processPlace
                .GetListWithPredicate(placeSearchParameter).Where(x => x.Building != null)];
            return View(placeList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(BuildingOperationParameterModel operationParameterModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage, int _) = processBuilding.Insert(operationParameterModel);
            return RedirectToAction(nameof(Index), new { statusMessage, statusCode });                    
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();

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
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage, int id) = processBuilding.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
