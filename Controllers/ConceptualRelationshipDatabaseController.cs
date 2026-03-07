using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class ConceptualRelationshipDatabaseController(
        IProcessConcept processConcept,
        IProcessConceptRelation processConceptRelation) : Controller
    {
        [HandleStatus]
        public ActionResult IndexGeneral(ConceptualRelationshipSearchParameterModel searchParameterModel)
        {
            searchParameterModel.RootConceptID = [null];
            List<ConceptViewModel> conceptList = [.. processConcept.Get(searchParameterModel).Select(x => x.ConceptViewModel)];
            return View(conceptList);
        }
        [HandleStatus]
        public ActionResult IndexSpecific(ConceptualRelationshipSearchParameterModel searchParameters)
        {
            List<ConceptViewModel> conceptList = [.. processConcept.Get(searchParameters).Select(x => x.ConceptViewModel)];
            return View(conceptList);
        }

        [HttpGet]
        [HandleStatus]
        public ActionResult Create(ConceptualRelationshipOperationParameterModel operationParameterModel)
        {
            ViewData["RootConceptID"] = operationParameterModel.ConceptViewModel.RootConceptID;
            return View(operationParameterModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateConfirmed(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (!ModelState.IsValid)
            {
                return View(conceptualRelationshipOperation);
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Insert(conceptualRelationshipOperation);

            if(rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID, rootConceptID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        [HttpGet]
        [HandleStatus]
        public ActionResult Edit(int id)
        {
            ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.Get(
                new ConceptualRelationshipSearchParameterModel {  Id = [id] }).FirstOrDefault();
            if (operationParameter == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound", id });
            }
            else 
            {
                @ViewData["RootConceptID"] = operationParameter.ConceptViewModel.RootConceptID;
                if (operationParameter.ConceptViewModel.RootConceptID > 0)
                {
                    operationParameter.ConceptRelationList = processConceptRelation.GetByConceptID(operationParameter.ConceptViewModel.Id);
                }
                return View(operationParameter);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return View(conceptualRelationshipOperationParameter);
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Update(conceptualRelationshipOperationParameter);
            
            if (rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID, rootConceptID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            ConceptualRelationshipOperationParameterModel? operationParameter = processConcept.Get(
                new ConceptualRelationshipSearchParameterModel { Id = [id] }).FirstOrDefault();
            return operationParameter == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ConceptualRelation_NotFound", id })
                : View(operationParameter);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(ConceptualRelationshipOperationParameterModel operationParameterModel)
        {
            if (!ModelState.IsValid)
            {
                return View(operationParameterModel);
            }
            (int _, string statusMessage) = processConcept.Delete(operationParameterModel.ConceptViewModel.Id);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}