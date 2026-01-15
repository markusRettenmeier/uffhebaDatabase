using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class SettlementDatabaseController(IProcessSettlement processSettlement,
        IProcessPlace processPlace) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Settlement != null)];
            return View(placeList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(SettlementOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int _) = processSettlement.Insert(model);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();

            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(new SettlementOperationParameterModel
                {
                    Place = existingPlace,
                    Settlement = existingPlace.Settlement!,
                    PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                    SettlementNPostalcodeList = existingPlace.Settlement!.SettlementNPostalcodeList,
                    ChildPlaceList = existingPlace.ChildPlaceList
                });
        }
        public IActionResult EditSubmit(SettlementOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processSettlement.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
