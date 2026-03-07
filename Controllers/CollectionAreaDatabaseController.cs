using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class CollectionAreaDatabaseController(IProcessCollectionArea processCollection) : Controller
    {
        [HandleStatus]
        public ActionResult Index(CollectionAreaSearchParameterModel collectionSearchParameterModel)
        {
            List<CollectionArea> collectionList = processCollection
                .GetListWithPredicate(collectionSearchParameterModel);

            return View(collectionList);
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CollectionArea collectionArea)
        {
            if (!ModelState.IsValid)
            {
                return View(collectionArea);
            }

            (int statusCode, string statusMessage, int collectionAreaID) = processCollection.Insert(collectionArea);
            if (collectionAreaID == 0)
                return RedirectToAction(nameof(Index), new { statusMessage });
            else
                return RedirectToAction("Create", "ConceptualRelationshipDatabase", new { statusMessage, statusCode, collectionAreaID });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] })
                .FirstOrDefault();

            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(existingCollection);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CollectionArea collectionArea)
        {
            if (!ModelState.IsValid)
            {
                return View(collectionArea);
            }

            (int statusCode, string statusMessage, int id) = processCollection.Update(collectionArea);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] })
                .FirstOrDefault();
            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(existingCollection);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });

            (int statusCode, string statusMessage) = processCollection.Delete(id);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}