using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class EraDatabaseController(IProcessEra processEra) : Controller
    {
        [HandleStatus]
        public ActionResult Index(EraSearchParameterModel eraSearchParameterModel)
        {
            return View(processEra.GetWithPredicates(eraSearchParameterModel));
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }        
        public ActionResult CreateSubmit(Era era)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int _, string statusMessage, int _) = processEra.Insert(era);

            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Era? era = processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                       .FirstOrDefault();
            return era == null 
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Era_NotFound" }) 
                : View(era);
        }
        public ActionResult EditSubmit(Era model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processEra.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
