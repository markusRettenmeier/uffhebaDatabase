using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class StatePreservationDatabaseController(IProcessStatePreservation processStates) : Controller
    {
        [HandleStatus]
        public IActionResult Index(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            ViewData["CollectionAreaID"] = stateSearchParameterModel.CollectionArea_CollectionAreaID[0];

            return View(processStates.GetWithPredicates(stateSearchParameterModel));
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create(int collectionAreaID)
        {
            StatePreservation state = new() { StatePreservationName = string.Empty, CollectionAreaID = collectionAreaID };
            return View(state);
        }
        [HttpPost]
        public IActionResult Create(StatePreservation state)
        {
            if (!ModelState.IsValid)
            {
                return View(state);
            }
            (int statusCode, string statusMessage) = processStates.Insert(state);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int statePreservationID)
        {
            StatePreservation existingState = processStates.GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [statePreservationID] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" })
                : View(existingState);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StatePreservation state)
        {
            if (!ModelState.IsValid)
            {
                return View(state);
            }
            (int statusCode, string statusMessage) = processStates.Update(state);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, state.CollectionAreaID });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            StatePreservation existingState = processStates.GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" })
                : View(existingState);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processStates.Delete(id);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
