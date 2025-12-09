using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CollectionAttributeDatabaseController(IProcessCollectionAttribute processCollectionAttribute
        , IProcessCollectionArea processCollectionArea,
            IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public ActionResult Index(Status status, CollectionAreaSearchParameterModel collectionAreaSearchParameter)
        {
            HandleStatus(status);

            CollectionArea? collectionList = processCollectionArea.GetListWithPredicate(collectionAreaSearchParameter).FirstOrDefault();
            return collectionList == null
                ? RedirectToAction("Index", "CollectionAreaDatabase", new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(collectionList);
        }

        public ActionResult Create(Status status, int collectionAreaID)
        {
            if (collectionAreaID == 0)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionAreaID_Missing" });
            }

            HandleStatus(status);

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

        public ActionResult Edit(Status status, int collectionAttributeID)
        {
            HandleStatus(status);

            CollectionAttribute? existingCollectionAttribute = processCollectionAttribute
                .GetListWithPredicate(new CollectionAttributeSearchParameterModel { CollectionAttributeID = [collectionAttributeID] })
                .FirstOrDefault();
            return existingCollectionAttribute == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionAttribute_Missing", collectionAttributeID })
                : View(existingCollectionAttribute);
        }
        public IActionResult EditSubmit(CollectionAttribute collectionAttribute)
        {
            (int _, int _, string statusMessage) = processCollectionAttribute.Edit(collectionAttribute);
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
