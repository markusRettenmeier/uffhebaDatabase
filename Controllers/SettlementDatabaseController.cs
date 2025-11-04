using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class SettlementDatabaseController(IProcessSettlement processSettlement,
        IProcessPlace processPlace) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["Place"] = "Settlement";

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.Settlement != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(SettlementOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processSettlement.CreateSettlement(model);
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
                return RedirectToAction(nameof(Index), new { statusMessage = "Siedlung nicht gefunden" });
            }

            SettlementOperationParameterModel settlementSelect = new()
            {
                Place = existingPlace,
                Settlement = existingPlace.Settlement!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                SettlementNPostalcodeList = existingPlace.Settlement!.SettlementNPostalcodeList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(settlementSelect);
        }
        public IActionResult EditSubmit(SettlementOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processSettlement.EditSettlement(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
