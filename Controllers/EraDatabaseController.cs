using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Services.Processes;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class EraDatabaseController(IProcessEra processEra) : Controller
    {
        public ActionResult Index(string statusMessage, EraSearchParameterModel eraSearchParameterModel)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View(processEra.GetWithPredicates(eraSearchParameterModel));
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }
        public ActionResult CreateSubmit(EraOperationParameterModel eraOperationParameterModel)
        {
            (Era _, int _, string statusMessage) = processEra.Create(eraOperationParameterModel);

            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            Era? era = (from e in processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                        select e).FirstOrDefault();
            return era == null ? RedirectToAction(nameof(Index), new { statusMessage = "Gebäude nicht gefunden" }) : View(era);
        }
        public ActionResult EditSubmit(EraOperationParameterModel model)
        {
            (Era era, int _, string statusMessage) = processEra.Edit(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = era.EraID });
        }
    }
}
