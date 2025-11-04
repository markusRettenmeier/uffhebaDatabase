using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Services.Processes.CollectionAreaProcesses;
using Sammlerplattform.Services.Processes.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    public class ConceptualRelationshipDatabaseController(IProcessConcept processConcept,
        IProcessCollectionArea processCollectionArea) : Controller
    {
        public ActionResult Index(string statusMessage, CollectionAreaSearchParameterModel collectionAreaSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["CollectionAreaID"] = statusMessage;

            CollectionArea? collectionArea = processCollectionArea.GetListWithPredicate(collectionAreaSearchParameter).FirstOrDefault();
            return View(collectionArea);
        }

        public ActionResult Create(string statusMessage, int collectionAreaID)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["CollectionAreaID"] = collectionAreaID;

            return View();
        }
        public IActionResult CreateSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            (int collectionAreaID, int _, string statusMessage) = processConcept.Insert(conceptualRelationshipOperation);
            return RedirectToAction(nameof(Create), new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            ConceptualRelationshipSearchParameterModel searchParameter = new()
            {
                ConceptID = [id]
            };
            ConceptualRelationshipOperationParameterModel? conceptualRelationshipOperationParameter = processConcept.GetWithPredicates(searchParameter).FirstOrDefault();
            return conceptualRelationshipOperationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Sammlungsgebiet nicht gefunden", id })
                : View(conceptualRelationshipOperationParameter);
        }
        public ActionResult EditSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperationParameter)
        {
            (int _, int _, string statusMessage) = processConcept.Update(conceptualRelationshipOperationParameter);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Delete(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            ConceptualRelationshipSearchParameterModel searchParameter = new()
            {
                ConceptID = [id]
            };
            ConceptualRelationshipOperationParameterModel? conceptualRelationshipOperationParameter = processConcept.GetWithPredicates(searchParameter).FirstOrDefault();
            return conceptualRelationshipOperationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Beziehung nicht gefunden", id })
                : View(conceptualRelationshipOperationParameter);
        }
        public ActionResult DeleteSubmit(ConceptualRelationshipOperationParameterModel operationParameterModel)
        {
            (int _, string statusMessage) = processConcept.DeleteConcept(operationParameterModel.Concept.ConceptID);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}