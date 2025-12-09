using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class StateDatabaseController(IProcessState processStates, IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public IActionResult Index(Status status, StateSearchParameterModel stateSearchParameterModel)
        {
            HandleStatus(status);
            ViewData["CollectionAreaID"] = stateSearchParameterModel.CollectionArea_CollectionAreaID[0];

            return View(processStates.GetWithPredicates(stateSearchParameterModel));
        }

        public ActionResult Create(Status status, int collectionAreaID)
        {
            HandleStatus(status);

            State state = new() { StateName = string.Empty, CollectionAreaID = collectionAreaID };
            return View(state);
        }
        public IActionResult CreateSubmit(State state)
        {
            (int collectionAreaID, string statusMessage) = processStates.Create(state);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(Status status, int stateID)
        {
            HandleStatus(status);

            State existingState = processStates.GetWithPredicates(new StateSearchParameterModel { StateID = [stateID] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_State_NotFound" })
                : View(existingState);
        }
        public ActionResult EditSubmit(State state)
        {
            (int collectionAreaID, string statusMessage) = processStates.Update(state);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }

        public ActionResult Delete(Status status, int stateID)
        {
            HandleStatus(status);

            State existingState = processStates.GetWithPredicates(new StateSearchParameterModel { StateID = [stateID] }).First();
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_State_NotFound" })
                : View(existingState);
        }
        public IActionResult DeleteSubmit(int stateID, int collectionAreaID)
        {
            (int _, string statusMessage) = processStates.Delete(stateID, collectionAreaID);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
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
