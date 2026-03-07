using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class CollectionSetDatabaseController(IProcessCollectionSet processSet) : Controller
    {
        [HandleStatus]
        public ActionResult Index(CollectionSetSearchParameterModel searchParameter)
        {
            List<CollectionSet> setList = processSet.GetWithPredicates(searchParameter);

            return View(setList);
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(CollectionSet set)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_ModelState_Invalid" });
            }
            (int statusCode, string statusMessage, int setID) = processSet.Insert(set);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            CollectionSet? set = processSet.GetWithPredicates(new CollectionSetSearchParameterModel { CollectionSetId = [id] }).FirstOrDefault();

            return set == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionSet_NotFound" })
                : View(set);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CollectionSet set)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_ModelState_Invalid" });
            }
            (int statusCode, string statusMessage, int id) = processSet.Update(set);
            if(statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            CollectionSet? set = processSet.GetWithPredicates(new CollectionSetSearchParameterModel { CollectionSetId = [id] }).FirstOrDefault();
            return set == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionSet_NotFound" })
                : View(set);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            (int statusCode, string statusMessage) = processSet.Delete(id);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}