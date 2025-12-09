using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionItemDatabaseController(IProcessCollectionItemEntity processCollectionItem,
        IProcessCollectionArea processCollectionArea,
        IProcessCollectionAttribute processCollectionAttribute,
        IProcessState processState,
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

            CollectionItemOperationParameterModel model = new()
            {
                CollectionItemEntity = new()
                {
                    UsingIdentityUsersID = string.Empty, // wird in CreateSubmit gesetzt
                    CollectionAreaID = collectionAreaID
                },
                CollectionItemPictureList = [new CollectionItemPicture()], // MINDESTENS EIN ELEMENT HINZUFÜGEN, weil CollectionItemPictureList[0] in Create
                CollectionAttributeValueList = [],
                CollectionAttributeList = processCollectionAttribute.GetListWithPredicate(new CollectionAttributeSearchParameterModel() { CollectionAreaID = [collectionAreaID] }),
                StateList = processState.GetWithPredicates(new StateSearchParameterModel() { CollectionArea_CollectionAreaID = [collectionAreaID] })
            };

            return View(model);
        }
        public IActionResult CreateSubmit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            collectionItemOperationParameter.CollectionItemEntity.UsingIdentityUsersID = userManager.GetUserId(User) ?? throw new NullReferenceException();
            string statusMessage = processCollectionItem.Insert(collectionItemOperationParameter);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        public ActionResult Edit(Status status, int entityId)
        {
            HandleStatus(status);

            CollectionItemOperationParameterModel? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();

            return existingCollectionItem == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" })
                : View(existingCollectionItem);
        }
        public ActionResult EditSubmit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            string statusMessage = processCollectionItem.Update(collectionItemOperationParameter);
            return RedirectToAction(nameof(Index), new { statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
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
            string statusMessage = processCollectionItem.Delete(collectionItemOperation);
            return RedirectToAction(nameof(Index), new { statusMessage, collectionItemOperation.CollectionItemEntity.CollectionAreaID });
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
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.Message];
                ViewData["StatusCode"] = status.Code;
            }
        }
    }
}
