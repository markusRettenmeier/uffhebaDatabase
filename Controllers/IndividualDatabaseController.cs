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

            List<Participant> participantList = [.. processParticipant.GetListWithPredicate(participantSearchParameter).Where(x => x.Individual != null)];
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
            Participant? existingParticipant = processParticipant
                .GetListWithPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();
            if (existingParticipant == null || existingParticipant.Individual == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Participant_NotFound" });
            }

            IndividualEditDTO individualEditDTO = new()
            {
                Id = existingParticipant.ParticipantID,
                Name = existingParticipant.ParticipantName,
                WikipediaUrl = existingParticipant.WikipediaUrl,
                Pseudonym = existingParticipant.Individual.Pseudonym,
                Signature = existingParticipant.Individual.Signature,
                BirthYear = existingParticipant.StartYear,
                DeathYear = existingParticipant.EndYear,
                ConnectedPlaceList = [.. existingParticipant.ParticipantNPlaceList.Select(x => new ConnectedPlaceDTO {
                    Id = x.PlaceID,
                    //Relationship = x.
                })]
            };

            ViewData["ConnectedPlaces"] = existingParticipant.ParticipantNPlaceList.Select(x => x.Place).ToList();
            ViewData["ConnectedEras"] = existingParticipant.ParticipantNEraList.Select(x => x.Era).ToList();

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
            Participant? existingParticipant = processParticipant
                .GetListWithPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] }).FirstOrDefault();
            return existingParticipant == null || existingParticipant.Individual == null
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
