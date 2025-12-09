using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class OrganizationDatabaseController(IProcessOrganization processOrganization, IProcessParty processParty,
        IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, PartySearchParameterModel partySearchParameter)
        {
            HandleStatus(status);
            ViewData["Party"] = "Organization";

            List<Party> partyList = [.. processParty.GetListWithPredicate(partySearchParameter).Where(x => x.Organization != null)];
            return View(partyList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);
            return View();
        }
        public IActionResult CreateSubmit(OrganizationOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processOrganization.CreateOrganization(model);
            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            Party? existingParty = processParty.GetListWithPredicate(new PartySearchParameterModel { PartyID = [id] }).FirstOrDefault();
            

            ;
            return existingParty == null || existingParty.Individual == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" })
                : View(new OrganizationOperationParameterModel
                {
                    Party = existingParty,
                    Organization = existingParty.Organization!,
                    PlaceList = existingParty.PlaceList,
                });
        }
        public IActionResult EditSubmit(OrganizationOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processOrganization.EditOrganization(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.Message];
                ViewData["StatusCode"] = status.Code;
            }
        }
    }
}
