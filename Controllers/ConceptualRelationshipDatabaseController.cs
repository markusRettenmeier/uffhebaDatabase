using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ConceptualRelationshipDatabaseController(
        IProcessConcept processConcept,
        IProcessConceptValue processConceptValue) : Controller
    {
        [HandleStatus]
        public ActionResult IndexGeneral(ConceptualRelationshipSearchParameterModel searchParameters)
        {
            searchParameters.RootConceptID = [null];
            List<ConceptViewModel> conceptDisplayList = [.. processConcept.Get(searchParameters).Select(x => x.ConceptViewModel)];
            return View(conceptDisplayList);
        }
        [HandleStatus]
        public IActionResult IndexSpecific(ConceptualRelationshipSearchParameterModel searchParameters)
        {
            ViewData["ConceptValueConceptIdList"] = processConceptValue.Get(new Models.ConceptualRelationshipDatabase.ConceptValueDatabase.ConceptValueSearchParameterModel
            {
                ConceptID = searchParameters.Id
            }).Select(x => x.ConceptID).ToList();
            List<ConceptViewModel> conceptList = [.. processConcept
                .Get(searchParameters)
                .Select(x => x.ConceptViewModel)];
            return View(conceptList);
        }

        public ActionResult Create(int rootConceptID, int collectionAreaID, int conceptTypeInt)
        {
            @ViewData["RootConceptID"] = rootConceptID;
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
            @ViewData["RootConceptID"] = displayDto.ConceptViewModel.RootConceptID;

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
                : View(displayDto.ConceptViewModel);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processConcept.Delete(id);
            return RedirectToAction(nameof(IndexGeneral), new { statusCode, statusMessage });
        }
    }
}