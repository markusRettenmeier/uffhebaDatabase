using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.EraDatabase;
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

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EraCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }
            (int statusCode, string statusMessage, int id) = processEra.Insert(createDTO);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Era? era = processEra.GetWithPredicates(new EraSearchParameterModel { EraID = [id] })
                       .FirstOrDefault();
            if (era == null)
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Era_NotFound" });

            EraEditDTO eraEditDTO = new()
            {
                Name = era.EraName,
                WikipediaUrl = era.WikipediaUrl,
                Id = era.EraID
            };
            return View(eraEditDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EraEditDTO editDto)
        {
            if (!ModelState.IsValid)
            {
                return View(editDto);
            }
            (int statusCode, string statusMessage, int id) = processEra.Update(editDto);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

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
        public ActionResult DeleteConfirmed(int eraId)
        {
            if (eraId <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processEra.Delete(eraId);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
