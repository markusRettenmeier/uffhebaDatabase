using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    public class ConceptualRelationshipDatabaseController(IProcessConcept processConcept,
        IProcessCollectionArea processCollectionArea,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, CollectionAreaSearchParameterModel collectionAreaSearchParameter)
        {
            HandleStatus(status);

            CollectionArea? collectionArea = processCollectionArea.GetListWithPredicate(collectionAreaSearchParameter).FirstOrDefault();
            return View(collectionArea);
        }

        public ActionResult Create(Status status, int collectionAreaID)
        {
            HandleStatus(status);
            ViewData["CollectionAreaID"] = collectionAreaID;

            return View();
        }
        public IActionResult CreateSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            (int collectionAreaID, int _, string statusMessage) = processConcept.Insert(conceptualRelationshipOperation);
            return RedirectToAction(nameof(Create), new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.GetWithPredicates(
                new ConceptualRelationshipSearchParameterModel {  ConceptID = [id] }).FirstOrDefault();
            return operationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound", id })
                : View(operationParameter);
        }
        public ActionResult EditSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperationParameter)
        {
            (int _, int _, string statusMessage) = processConcept.Update(conceptualRelationshipOperationParameter);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Delete(Status status, int id)
        {
            HandleStatus(status);

            ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.GetWithPredicates(
                new ConceptualRelationshipSearchParameterModel { ConceptID = [id] }).FirstOrDefault();
            return operationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ConceptualRelation_NotFound", id })
                : View(operationParameter);
        }
        public ActionResult DeleteSubmit(ConceptualRelationshipOperationParameterModel operationParameterModel)
        {
            (int _, string statusMessage) = processConcept.DeleteConcept(operationParameterModel.Concept.Id);
            return RedirectToAction(nameof(Index), new { statusMessage });
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