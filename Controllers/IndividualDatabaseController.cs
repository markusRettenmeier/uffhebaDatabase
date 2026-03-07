using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class IndividualDatabaseController(IProcessIndividual processIndividual,
        IProcessParty processParty) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PartySearchParameterModel partySearchParameter)
        {
            ViewData["Party"] = "Individual";

            List<Party> partyList = [.. processParty.GetListWithPredicate(partySearchParameter).Where(x => x.Individual != null)];
            return View(partyList);
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IndividualCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            (int statusCode, string statusMessage, int _) = processIndividual.Insert(model);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Party? existingParty = processParty
                .GetListWithPredicate(new PartySearchParameterModel{ PartyID = [id] }).FirstOrDefault();
            if (existingParty == null || existingParty.Individual == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" });
            }

            IndividualEditDTO individualEditDTO = new()
            {
                PartyID = existingParty.PartyID,
                PartyTypeInt = existingParty.PartyTypeInt,
                Name = existingParty.PartyName,
                WikipediaUrl = existingParty.WikipediaUrl,
                Pseudonym = existingParty.Individual.Pseudonym,
                Signature = existingParty.Individual.Signature
            };

            //ViewData["ConnectedPlaces"] = existingParty.PlaceList;

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

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Party? existingParty = processParty
                .GetListWithPredicate(new PartySearchParameterModel { PartyID = [id] }).FirstOrDefault();
            if (existingParty == null || existingParty.Individual == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" });
            }
            return View(existingParty);
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
