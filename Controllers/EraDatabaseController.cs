using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Services.Processes;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class EraDatabaseController(IProcessEra processEra) : Controller
    {
        public ActionResult AdministerCollectionEra(string statusMessage, EraSearchParameterModel eraSearchParameterModel)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View(processEra.GetWithPredicates(eraSearchParameterModel));
        }

        public ActionResult CreateEra(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public ActionResult CreateEraSubmit(EraOperationParameterModel eraOperationParameterModel)
        {
            (Era _, int _, string statusMessage) = processEra.Create(eraOperationParameterModel);

            return RedirectToAction(nameof(CreateEra), new { statusMessage });
        }

        public ActionResult EditEra(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            Era? era = (from e in processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                        select e).FirstOrDefault();

            return View(era);
        }

        public ActionResult EditÉraSubmit(EraOperationParameterModel model)
        {
            (Era era, int _, string statusMessage) = processEra.Edit(model);

            return RedirectToAction(nameof(EditEra), new { statusMessage, id = era.EraID });
        }
    }
}
