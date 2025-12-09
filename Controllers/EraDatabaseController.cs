using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Models;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class EraDatabaseController(IProcessEra processEra,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, EraSearchParameterModel eraSearchParameterModel)
        {
            HandleStatus(status);

            return View(processEra.GetWithPredicates(eraSearchParameterModel));
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);

            return View();
        }
        public ActionResult CreateSubmit(EraOperationParameterModel eraOperationParameterModel)
        {
            (Era _, int _, string statusMessage) = processEra.Create(eraOperationParameterModel);

            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            Era? era = processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                       .FirstOrDefault();
            return era == null 
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Era_NotFound" }) 
                : View(era);
        }
        public ActionResult EditSubmit(EraOperationParameterModel model)
        {
            (Era era, int _, string statusMessage) = processEra.Edit(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = era.EraID });
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
