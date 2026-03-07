using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class OrganizationDatabaseController(IProcessOrganization processOrganization
        , IProcessParty processParty) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PartySearchParameterModel partySearchParameter)
        {
            ViewData["Party"] = "Organization";

            List<Party> partyList = [.. processParty.GetListWithPredicate(partySearchParameter).Where(x => x.Organization != null)];
            return View(partyList);
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(OrganizationCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            (int statusCode, string statusMessage, int _) = processOrganization.Insert(model);
            return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Party? existingParty = processParty.GetListWithPredicate(new PartySearchParameterModel { PartyID = [id] }).FirstOrDefault();

            return existingParty == null || existingParty.Individual == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" })
                : View(new OrganizationOperationParameterModel
                {
                    Party = existingParty,
                    Organization = existingParty.Organization!,
                    //PlaceList = existingParty.PlaceList,
                });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrganizationOperationParameterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            (int statusCode, string statusMessage, int id) = processOrganization.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Party? existingParty = processParty.GetListWithPredicate(new PartySearchParameterModel { PartyID = [id] }).FirstOrDefault();

            return existingParty == null || existingParty.Individual == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Party_NotFound" })
                : View(new OrganizationOperationParameterModel
                {
                    Party = existingParty,
                    Organization = existingParty.Organization!,
                    //PlaceList = existingParty.PlaceList,
                });
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
