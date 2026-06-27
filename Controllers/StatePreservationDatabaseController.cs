using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class StatePreservationDatabaseController(
        IProcessStatePreservation processStates,
        IProcessCollectionArea processCollectionArea) : Controller
    {
        [HandleStatus]
        public IActionResult Index(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            ViewData["CollectionAreaID"] = stateSearchParameterModel.CollectionArea_CollectionAreaID[0];
            string? collectionAreaName = processCollectionArea.GetWithTranslationsListViaPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = stateSearchParameterModel.CollectionArea_CollectionAreaID }).Select(x => x.CollectionAreaName).FirstOrDefault();
            ViewData["CollectionAreaName"] = collectionAreaName ?? string.Empty;

            return View(processStates.GetWithTranslationsListViaPredicates(stateSearchParameterModel));
        }

        public ActionResult Create(int collectionAreaID)
        {
            StatePreservationCreateDTO createDTO = new()
            {
                Name = string.Empty,
                CollectionAreaID = collectionAreaID
            };
            return View(createDTO);
        }
        [HttpPost]
        public IActionResult Create(StatePreservationCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }
            (int statusCode, string statusMessage, int id) = processStates.Insert(createDTO);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode, CollectionArea_CollectionAreaID = createDTO.CollectionAreaID });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            StatePreservationDisplayDTO existingState = processStates.GetWithTranslationsListViaPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).First();
            if (existingState == null)
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" });

            StatePreservationEditDTO editDTO = new()
            {
                Id = existingState.Id,
                Name = existingState.Name,
                CollectionAreaID = existingState.CollectionAreaID,
                SortingOrder = existingState.SortingOrder
            };
            return View(editDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StatePreservationEditDTO editDto)
        {
            if (!ModelState.IsValid)
            {
                return View(editDto);
            }
            (int statusCode, string statusMessage, int id) = processStates.Update(editDto);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage, CollectionArea_CollectionAreaID = editDto.CollectionAreaID });
        }

        public ActionResult Delete(int id)
        {
            StatePreservationDisplayDTO existingState = processStates.GetWithTranslationsListViaPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).First();
            
            string? collectionAreaName = processCollectionArea.GetWithTranslationsListViaPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [existingState.CollectionAreaID] }).Select(x => x.CollectionAreaName).FirstOrDefault();
            ViewData["CollectionAreaName"] = collectionAreaName ?? string.Empty;
            
            return existingState == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_StatePreservation_NotFound" })
                : View(existingState);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int statePreservationId, int collectionAreaId)
        {
            if (statePreservationId <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processStates.Delete(statePreservationId);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, CollectionArea_CollectionAreaID = collectionAreaId });
        }
    }
}
