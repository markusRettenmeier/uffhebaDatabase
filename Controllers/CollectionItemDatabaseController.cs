using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionItemDatabaseController(IProcessCollectionItemEntity processCollectionItem,
        IProcessCollectionArea processCollectionArea,
        IProcessStatePreservation processState,
        UserManager<UsingIdentityUser> userManager,
        IWebHostEnvironment hostEnvironment,
        IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        [Authorize]
        public ActionResult Index(Status status, CollectionItemSearchParameterModel model)
        {
            HandleStatus(status);

            ViewData["CollectionArea"] = processCollectionArea.GetListWithPredicate(new CollectionAreaSearchParameterModel() { CollectionAreaID = model.CollectionAreaID }).FirstOrDefault();

            string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();
            model.UsingIdentityUsersID.Add(userId);

            return View(processCollectionItem.GetWithPredicates(model));
        }

        public ActionResult Create(Status status, int collectionAreaID)
        {
            HandleStatus(status);
            ViewData["CollectionAreaID"] = collectionAreaID;

            CollectionItemOperationParameterModel model = new()
            {
                CollectionItemEntity = new()
                {
                    UsingIdentityUsersID = string.Empty, // wird in CreateSubmit gesetzt
                    CollectionAreaID = collectionAreaID
                },
                CollectionItemPictureList = [new CollectionItemPicture()], // MINDESTENS EIN ELEMENT HINZUFÜGEN, weil CollectionItemPictureList[0] in Create
                ConceptValueList = [],
                StatePreservationList = processState.GetWithPredicates(new StatePreservationSearchParameterModel() { CollectionArea_CollectionAreaID = [collectionAreaID] })
            };

            return View(model);
        }
        public IActionResult CreateSubmit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            collectionItemOperationParameter.CollectionItemEntity.UsingIdentityUsersID = userManager.GetUserId(User) ?? throw new NullReferenceException();
            (int statusCode, string statusMessage) = processCollectionItem.Insert(collectionItemOperationParameter);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        public ActionResult Edit(Status status, int entityId)
        {
            HandleStatus(status);

            CollectionItemOperationParameterModel? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();
            if (existingCollectionItem != null)
            {
                ViewData["CollectionAreaID"] = existingCollectionItem.CollectionItemEntity.CollectionAreaID;
            }

            return existingCollectionItem == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" })
                : View(existingCollectionItem);
        }
        public ActionResult EditSubmit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage) = processCollectionItem.Update(collectionItemOperationParameter);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        public ActionResult Delete(Status status, int entityId)
        {
            HandleStatus(status);

            CollectionItemOperationParameterModel? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();

            return existingCollectionItem == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" })
                : View(existingCollectionItem);
        }
        public IActionResult DeleteSubmit(CollectionItemOperationParameterModel collectionItemOperation)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }

            (int statusCode, string statusMessage) = processCollectionItem.Delete(collectionItemOperation);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionItemOperation.CollectionItemEntity.CollectionAreaID });
        }

        public async Task<ActionResult> DownloadSubmit(int? entityId)
        {
            Task<UsingIdentityUser?> userTask = userManager.GetUserAsync(User);
            UsingIdentityUser? user = await userTask;
            if (user == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_User_NotFound" });
            }

            CollectionItemSearchParameterModel collectionItemSearch = new();
            if (entityId > 0)
            {
                collectionItemSearch.CollectionItemEntityID.Add((int)entityId);
            }
            collectionItemSearch.UsingIdentityUsersID.Add(user.Id);

            // Hier liegt das Problem - Sie müssen sicherstellen, dass jedes CollectionItem einzeln verarbeitet wird
            List<CollectionItemOperationParameterModel> modelList = [.. processCollectionItem.GetWithPredicates(collectionItemSearch)];

            MemoryStream memory = await YamlProcessor.CreateZipFile(modelList, user, hostEnvironment);

            return File(memory, "application/zip", "Download_" + user.UserName + ".zip");
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.StatusMessage))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.StatusMessage];
                ViewData["StatusCode"] = status.StatusCode;
            }
        }
    }
}
