using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Services.Processes.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionAreaDatabaseController(IProcessCollectionArea processCollection) : Controller
    {
        public ActionResult Index(string statusMessage, CollectionAreaSearchParameterModel collectionSearchParameterModel)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<CollectionArea> collectionList = processCollection.GetListWithPredicate(collectionSearchParameterModel);
            return View(collectionList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(string collectionAreaName)
        {
            (int collectionAreaID, int _, string statusMessage) = processCollection.Create(collectionAreaName);
            return RedirectToAction("Create", "CollectionAttributeDatabase", new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionAreaSearchParameterModel searchParameter = new()
            {
                CollectionAreaID = [id]
            };
            CollectionArea? existingCollection = processCollection.GetListWithPredicate(searchParameter).FirstOrDefault();
            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Sammlungsgebiet nicht gefunden", id })
                : View(existingCollection);
        }
        public IActionResult EditSubmit(CollectionArea collectionArea)
        {
            (int _, int _, string statusMessage) = processCollection.Edit(collectionArea);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}
