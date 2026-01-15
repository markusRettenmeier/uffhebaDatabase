using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    public class ConceptualRelationshipDatabaseController(IProcessConcept processConcept) : Controller
    {
        [HandleStatus]
        public ActionResult IndexSpecific(ConceptualRelationshipSearchParameterModel conceptSearchParameter)
        {
            ViewData["IsSpecific"] = true;

            List<Concept> conceptRelationList = [.. processConcept.GetWithPredicates(conceptSearchParameter).Select(x => x.Concept)];
            return View(conceptRelationList);
        }
        [HandleStatus]
        public ActionResult IndexGeneral(ConceptualRelationshipSearchParameterModel searchParameterModel)
        {
            searchParameterModel.CollectionAreaID = [0]; // IF not coll
            List<Concept> conceptList = [.. processConcept.GetWithPredicates(searchParameterModel).Select(x => x.Concept)];
            return View(conceptList);
        }

        [HandleStatus]
        public ActionResult Create(ConceptualRelationshipOperationParameterModel operationParameterModel)
        {
            return View(operationParameterModel);
        }
        public IActionResult CreateSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Insert(conceptualRelationshipOperation);

            if(rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID });
            }
            else if(collectionAreaID.HasValue)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, collectionAreaID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.GetWithPredicates(
                new ConceptualRelationshipSearchParameterModel {  Id = [id] }).FirstOrDefault();
            return operationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound", id })
                : View(operationParameter);
        }
        public IActionResult EditSubmit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Update(conceptualRelationshipOperationParameter);
            
            if (rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID });
            }
            else if (collectionAreaID.HasValue)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, collectionAreaID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        //public ActionResult Delete(Status status, int id)
        //{
        //    HandleStatus(status);

        //    ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.GetWithPredicates(
        //        new ConceptualRelationshipSearchParameterModel { Id = [id] }).FirstOrDefault();
        //    return operationParameter == null
        //        ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ConceptualRelation_NotFound", id })
        //        : View(operationParameter);
        //}
        //public IActionResult DeleteSubmit(ConceptualRelationshipOperationParameterModel operationParameterModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
        //    }
        //    (int _, string statusMessage) = processConcept.Delete(operationParameterModel.Concept.Id);
        //    return RedirectToAction(nameof(Index), new { statusMessage });
        //}
    }
}