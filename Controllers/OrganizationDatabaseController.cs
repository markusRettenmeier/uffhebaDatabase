using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class OrganizationDatabaseController(IProcessOrganization processOrganization
        , IProcessParticipant processParticipant) : Controller
    {
        [HandleStatus]
        public ActionResult Index(ParticipantSearchParameterModel participantSearchParameter)
        {
            ViewData["Participant"] = "Organization";

            List<ParticipantDisplayDTO> participantList = [.. processParticipant.GetTranslationsListViaPredicate(participantSearchParameter)];
            return View(participantList);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(OrganizationCreateDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }
            (int statusCode, string statusMessage, int id) = processOrganization.Insert(createDto);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            ParticipantDisplayDTO? existingParticipant = processParticipant.GetTranslationsListViaPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();
            if (existingParticipant == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Participant_NotFound" });
            }

            ViewData["ConnectedPlaces"] = existingParticipant.ConnectedPlaceList.ToList();
            ViewData["ConnectedEras"] = existingParticipant.ConnectedEraList.ToList();

            OrganizationEditDTO organizationEditDTO = new()
            {
                Id = existingParticipant.ParticipantID,
                Name = existingParticipant.Name,
                WikipediaUrl = existingParticipant.WikipediaUrl,
                Industry = existingParticipant.IndustryName
            };
            return View(organizationEditDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrganizationEditDTO editDto)
        {
            if (!ModelState.IsValid)
            {
                return View(editDto);
            }
            (int statusCode, string statusMessage, int id) = processOrganization.Update(editDto);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        public ActionResult Delete(int id)
        {
            ParticipantDisplayDTO? existingParticipant = processParticipant.GetTranslationsListViaPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();

            return existingParticipant == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Participant_NotFound" })
                : View(existingParticipant);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processOrganization.Delete(id);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
