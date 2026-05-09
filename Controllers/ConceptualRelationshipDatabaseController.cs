using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ConceptualRelationshipDatabaseController(
        IProcessConcept processConcept) : Controller
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

        public ActionResult Create(int rootConceptID, int collectionAreaID, int conceptTypeInt)
        {
            ConceptCreateDTO createDTO = new()
            {
                RootConceptID = rootConceptID > 0 ? rootConceptID : null,
                CollectionAreaID = collectionAreaID > 0 ? collectionAreaID : null,
                ConceptTypeInt = conceptTypeInt > 0 ? conceptTypeInt : null
            };
            return View(createDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ConceptCreateDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Insert(createDto);

            if (rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID, rootConceptID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            ConceptDisplayDTO? displayDto = processConcept.Get(
                new ConceptualRelationshipSearchParameterModel { Id = [id] }).FirstOrDefault();
            if (displayDto == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound", id });
            }

            ConceptEditDTO conceptEditDTO = new()
            {
                Id = displayDto.ConceptViewModel.Id,
                Name = displayDto.ConceptViewModel.Name,
                Abbreviation = displayDto.ConceptViewModel.Abbreviation,
                CollectionAreaID = displayDto.ConceptViewModel.CollectionAreaID,
                ConceptTypeInt = displayDto.ConceptViewModel.ConceptTypeInt,
                RootConceptID = displayDto.ConceptViewModel.RootConceptID,
                ConceptRelationList = [.. displayDto.ConceptRelationViewList.Select(r => new ConceptRelationEditDTO
                {
                    ToConceptId = r.ToConceptID,
                    RelationTypeInt = r.RelationTypeInt,
                    ToName = r.ToConcept?.Name
                })]
            };
            return View(conceptEditDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ConceptEditDTO editDto)
        {
            if (!ModelState.IsValid)
            {
                return View(editDto);
            }
            (int rootConceptID, int? collectionAreaID, int statusCode, string statusMessage) = processConcept.Update(editDto);

            if (rootConceptID > 0)
            {
                return RedirectToAction(nameof(IndexSpecific), new { statusCode, statusMessage, Id = rootConceptID, rootConceptID });
            }
            else
                return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }

        public ActionResult Delete(int id)
        {
            ConceptDisplayDTO? displayDto = processConcept.Get(
                new ConceptualRelationshipSearchParameterModel { Id = [id] }).FirstOrDefault();
            return displayDto == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ConceptualRelation_NotFound", id })
                : View(displayDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(ConceptDisplayDTO displayDto)
        {
            if (!ModelState.IsValid)
            {
                return View(displayDto);
            }
            (int _, string statusMessage) = processConcept.Delete(displayDto.ConceptViewModel.Id);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}