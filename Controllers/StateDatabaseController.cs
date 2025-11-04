using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Services.Processes.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class StateDatabaseController(IProcessState processStates) : Controller
    {
        public IActionResult Index(string statusMessage, StateSearchParameterModel stateSearchParameterModel)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["CollectionAreaID"] = stateSearchParameterModel.CollectionArea_CollectionAreaID[0];

            return View(processStates.GetWithPredicates(stateSearchParameterModel));
        }

        public ActionResult Create(string statusMessage, int collectionAreaID)
        {
            ViewData["StatusMessage"] = statusMessage;

            State state = new() { StateName = string.Empty, CollectionAreaID = collectionAreaID };
            return View(state);
        }
        public IActionResult CreateSubmit(State state)
        {
            (int collectionAreaID, string statusMessage) = processStates.Create(state);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(string statusMessage, int stateID)
        {
            ViewData["StatusMessage"] = statusMessage;

            StateSearchParameterModel stateSearch = new();
            stateSearch.StateID.Add(stateID);
            State state = processStates.GetWithPredicates(stateSearch).First();
            return View(state);
        }
        public ActionResult EditSubmit(State state)
        {
            (int collectionAreaID, string statusMessage) = processStates.Update(state);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }

        public ActionResult Delete(string statusMessage, int stateID)
        {
            ViewData["StatusMessage"] = statusMessage;

            StateSearchParameterModel stateSearch = new();
            stateSearch.StateID.Add(stateID);
            return View(processStates.GetWithPredicates(stateSearch).First());
        }
        public IActionResult DeleteSubmit(int stateID, int collectionAreaID)
        {
            (int _, string statusMessage) = processStates.Delete(stateID, collectionAreaID);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }
    }
}
