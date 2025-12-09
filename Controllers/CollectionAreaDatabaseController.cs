using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionAreaDatabaseController(IProcessCollectionArea processCollection,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, CollectionAreaSearchParameterModel collectionSearchParameterModel)
        {
            HandleStatus(status);

            List<CollectionArea> collectionList = processCollection
                .GetListWithPredicate(collectionSearchParameterModel);

            return View(collectionList);
        }

        public ActionResult Create(Status status)
        {
            HandleStatus(status);
            return View();
        }
        public IActionResult CreateSubmit(string collectionAreaName)
        {
            (int collectionAreaID, int _, string statusMessage) = processCollection.Create(collectionAreaName);
            return RedirectToAction("Create", "CollectionAttributeDatabase", new { statusMessage, collectionAreaID });
        }

        public ActionResult Edit(Status status, int id)
        {
            HandleStatus(status);

            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel{ CollectionAreaID = [id]})
                .FirstOrDefault();

            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(existingCollection);
        }
        public IActionResult EditSubmit(CollectionArea collectionArea)
        {
            (int _, int _, string statusMessage) = processCollection.Edit(collectionArea);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.Message];
                ViewData["StatusCode"] = status.Code;
            }
        }
    }
}
