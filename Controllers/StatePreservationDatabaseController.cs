using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class StatePreservationDatabaseController(IProcessStatePreservation processStates) : Controller
    {
        [HandleStatus]
        public IActionResult Index(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            ViewData["CollectionAreaID"] = stateSearchParameterModel.CollectionArea_CollectionAreaID[0];

            return View(processStates.GetWithPredicates(stateSearchParameterModel));
        }

        [HandleStatus]
        public ActionResult Create(int collectionAreaID)
        {
            StatePreservation state = new() { StatePreservationName = string.Empty, CollectionAreaID = collectionAreaID };
            return View(state);
        }
        public IActionResult CreateSubmit(StatePreservation state)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage) = processStates.Insert(state);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int statePreservationID)
        {
            StatePreservation existingState = processStates.GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [statePreservationID] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" })
                : View(existingState);
        }
        public ActionResult EditSubmit(StatePreservation state)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage) = processStates.Update(state);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, state.CollectionAreaID });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Delete(int statePreservationID)
        {
            StatePreservation existingState = processStates.GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [statePreservationID] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" })
                : View(existingState);
        }
        public IActionResult DeleteSubmit(int statePreservationID, int collectionAreaID)
        {
            (int statusCode, string statusMessage) = processStates.Delete(statePreservationID, collectionAreaID);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionAreaID });
        }
    }
}
