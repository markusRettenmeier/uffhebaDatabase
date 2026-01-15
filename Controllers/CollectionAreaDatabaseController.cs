using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
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
        public ActionResult Create()
        {
            return View();
        }
        public IActionResult CreateSubmit(CollectionArea collectionArea)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage, int collectionAreaID) = processCollection.Insert(collectionArea);
            if (collectionAreaID == 0)
                return RedirectToAction(nameof(Index), new { statusMessage });
            else
                return RedirectToAction("Create", "ConceptualRelationshipDatabase", new { statusMessage, statusCode, collectionAreaID });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel{ CollectionAreaID = [id] })
                .FirstOrDefault();

            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(existingCollection);
        }
        public IActionResult EditSubmit(CollectionArea collectionArea)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage, int id) = processCollection.Update(collectionArea);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
