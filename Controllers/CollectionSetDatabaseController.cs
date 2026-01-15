using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionSetDatabaseController(IProcessCollectionSet processSet) : Controller
    {
        [HandleStatus]
        public ActionResult Index(CollectionSetSearchParameterModel searchParameter)
        {
            List<CollectionSet> setList = processSet.GetWithPredicates(searchParameter);

            return View(setList);
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        public ActionResult CreateSubmit(CollectionSet set)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int setID) = processSet.Insert(set);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            CollectionSet? set = processSet.GetWithPredicates(new CollectionSetSearchParameterModel { CollectionSetId = [id] }).FirstOrDefault();

            return set == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionSet_NotFound" })
                : View(set);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSubmit(CollectionSet set)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id) = processSet.Update(set);
            if(statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        // GET: SetDatabaseeController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: SetDatabaseeController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
