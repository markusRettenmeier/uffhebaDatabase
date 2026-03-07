using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class EraDatabaseController(IProcessEra processEra) : Controller
    {
        [HandleStatus]
        public ActionResult Index(EraSearchParameterModel eraSearchParameterModel)
        {
            return View(processEra.GetWithPredicates(eraSearchParameterModel));
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Era era)
        {
            if (!ModelState.IsValid)
            {
                return View(era);
            }
            (int _, string statusMessage, int _) = processEra.Insert(era);

            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Era? era = processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                       .FirstOrDefault();
            return era == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Era_NotFound" })
                : View(era);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Era era)
        {
            if (!ModelState.IsValid)
            {
                return View(era);
            }
            (int statusCode, string statusMessage, int id) = processEra.Update(era);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Era? era = processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                       .FirstOrDefault();
            return era == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Era_NotFound" })
                : View(era);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processEra.Delete(id);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
