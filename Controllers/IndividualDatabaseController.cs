using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
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
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(IndividualOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int _) = processIndividual.Insert(model);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Party? existingParty = processParty.GetListWithPredicate(new PartySearchParameterModel{ PartyID = [id] }).FirstOrDefault();
            
            return existingParty == null || existingParty.Individual == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" })
                : View(new IndividualOperationParameterModel
                {
                    Party = existingParty,
                    Individual = existingParty.Individual!,
                    PlaceList = existingParty.PlaceList
                }); 
        }
        public IActionResult EditSubmit(IndividualOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processIndividual.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
