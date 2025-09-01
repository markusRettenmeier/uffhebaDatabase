using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Services.Processes.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    public class OrganizationDatabaseController(IProcessOrganization processOrganization, IProcessParty processParty) : Controller
    {
        public ActionResult Index(string statusMessage, PartySearchParameterModel partySearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["Party"] = "Organization";

            List<Party> partyList = [.. processParty.GetListWithPredicate(partySearchParameter).Where(x => x.Organization != null)];
            return View(partyList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(OrganizationOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processOrganization.CreateOrganization(model);
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
            if (existingParty == null || existingParty.Organization == null)
            {
                return NotFound("Individuum nicht gefunden.");
            }

            OrganizationOperationParameterModel organizationOperationParameterModel = new()
            {
                Party = existingParty,
                Organization = existingParty.Organization!,
                PlaceList = existingParty.PlaceList,
            };
            return View(organizationOperationParameterModel);
        }
        public IActionResult EditSubmit(OrganizationOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processOrganization.EditOrganization(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
