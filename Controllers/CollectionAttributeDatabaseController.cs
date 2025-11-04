using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Services.Processes.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CollectionAttributeDatabaseController(IProcessCollectionAttribute processCollectionAttribute, IProcessCollectionArea processCollectionArea) : Controller
    {
        public ActionResult Index(string statusMessage, CollectionAreaSearchParameterModel collectionAreaSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionArea? collectionList = processCollectionArea.GetListWithPredicate(collectionAreaSearchParameter).FirstOrDefault();
            return collectionList == null
                ? RedirectToAction("Index", "CollectionAreaDatabase", new { statusMessage = "Sammlungsgebiet nicht gefunden." })
                : View(collectionList);
        }

        public ActionResult Create(string statusMessage, int collectionAreaID)
        {
            ViewData["StatusMessage"] = statusMessage;

            if (collectionAreaID == 0)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Ungültige Sammlung." });
            }
            CollectionAttribute collectionAttribute = new()
            {
                CollectionAttributeName = string.Empty,
                CollectionAreaID = collectionAreaID
            };
            return View(collectionAttribute);
        }
        public IActionResult CreateSubmit(CollectionAttribute collectionAttribute)
        {
            (int collectionAreaID, int _, string statusMessage) = processCollectionAttribute.Create(collectionAttribute);
            return RedirectToAction(nameof(Index), new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(string statusMessage, int collectionAttributeID)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionAttributeSearchParameterModel searchParameter = new()
            {
                CollectionAttributeID = [collectionAttributeID]
            };
            CollectionAttribute? existingCollectionAttribute = processCollectionAttribute.GetListWithPredicate(searchParameter).FirstOrDefault();
            return existingCollectionAttribute == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Attribut nicht gefunden", collectionAttributeID })
                : View(existingCollectionAttribute);
        }
        public IActionResult EditSubmit(CollectionAttribute collectionAttribute)
        {
            (int _, int _, string statusMessage) = processCollectionAttribute.Edit(collectionAttribute);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}
