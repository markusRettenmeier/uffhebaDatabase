using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class IndividualDatabaseController(IProcessIndividual processIndividual,
        IProcessParticipant processParticipant) : Controller
    {
        [HandleStatus]
        public ActionResult Index(ParticipantSearchParameterModel participantSearchParameter)
        {
            ViewData["Participant"] = "Individual";

            List<ParticipantDisplayDTO> participantList = [.. processParticipant.GetTranslationsListViaPredicate(participantSearchParameter)];
            return View(participantList);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IndividualCreateDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }
            (int statusCode, string statusMessage, int id) = processIndividual.Insert(createDto);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            ParticipantDisplayDTO? existingParticipant = processParticipant
                .GetTranslationsListViaPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();
            if (existingParticipant == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Participant_NotFound" });
            }

            IndividualEditDTO individualEditDTO = new()
            {
                Id = existingParticipant.ParticipantID,
                Name = existingParticipant.Name,
                WikipediaUrl = existingParticipant.WikipediaUrl,
                Pseudonym = existingParticipant.Pseudonym,
                Signature = existingParticipant.Signature,
                BirthYear = existingParticipant.StartYear,
                DeathYear = existingParticipant.EndYear,
                ConnectedPlaceList = [.. existingParticipant.ConnectedPlaceList.Select(x => new ConnectedPlaceDTO {
                    Id = x.PlaceID
                })]
            };

            ViewData["ConnectedPlaces"] = existingParticipant.ConnectedPlaceList.ToList();
            ViewData["ConnectedEras"] = existingParticipant.ConnectedEraList.ToList();

            return View(individualEditDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IndividualEditDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            (int statusCode, string statusMessage, int id) = processIndividual.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        public ActionResult Delete(int id)
        {
            ParticipantDisplayDTO? existingParticipant = processParticipant
                .GetTranslationsListViaPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();
            return existingParticipant == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Participant_NotFound" })
                : View(existingParticipant);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });

            (int statusCode, string statusMessage) = processIndividual.Delete(id);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
