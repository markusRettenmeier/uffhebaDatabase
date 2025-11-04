using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Services.Processes.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class IndividualDatabaseController(IProcessIndividual processIndividual,
        IProcessParty processParty) : Controller
    {
        public ActionResult Index(string statusMessage, PartySearchParameterModel partySearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["Party"] = "Individual";

            List<Party> partyList = [.. processParty.GetListWithPredicate(partySearchParameter).Where(x => x.Individual != null)];
            return View(partyList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(IndividualOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processIndividual.Create(model);
            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            PartySearchParameterModel searchParameterModel = new()
            {
                PartyID = [id]
            };
            Party? existingParty = processParty.GetListWithPredicate(searchParameterModel).FirstOrDefault();
            if (existingParty == null || existingParty.Individual == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Individuum nicht gefunden" });
            }

            IndividualOperationParameterModel individualOperationParameterModel = new()
            {
                Party = existingParty,
                Individual = existingParty.Individual!,
                PlaceList = existingParty.PlaceList
            };
            return View(individualOperationParameterModel);
        }
        public IActionResult EditSubmit(IndividualOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processIndividual.Edit(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
