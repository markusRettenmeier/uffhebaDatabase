using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.OwnershipProofPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
    public class CollectionItemDatabaseController(IProcessCollectionItemEntity processCollectionItem,
        IProcessCollectionArea processCollectionArea,
        IProcessStatePreservation processState,
        UserManager<UsingIdentityUser> userManager,
        IWebHostEnvironment hostEnvironment) : Controller
    {
        [HandleStatus]
        //public ActionResult Index(Status status, CollectionItemSearchParameterModel model)
        public ActionResult Index(CollectionItemSearchParameterModel model)
        {
            //HandleStatus(status);
            ViewData["CollectionArea"] = processCollectionArea.GetListWithPredicate(new CollectionAreaSearchParameterModel() { CollectionAreaID = model.CollectionAreaID }).FirstOrDefault();

            string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();
            model.UsingIdentityUsersID.Add(userId);

            return View(processCollectionItem.GetWithPredicates(model));
        }

        [HttpGet]
        [HandleStatus]
        public ActionResult Create(int collectionAreaID)
        {
            ViewData["CollectionAreaID"] = collectionAreaID;

            CollectionItemOperationParameterModel model = new()
            {
                CollectionItemEntity = new()
                {
                    UsingIdentityUsersID = string.Empty, // wird in Create gesetzt
                    CollectionAreaID = collectionAreaID
                },
                CollectionItemPictureList = [new CollectionItemPicture()], // MINDESTENS EIN ELEMENT HINZUFÜGEN, weil CollectionItemPictureList[0] in Create
                OwnershipProofPictureList = [new OwnershipProofPicture()], // MINDESTENS EIN ELEMENT HINZUFÜGEN, weil OwnershipProofPictureList[0] in Create
                ConceptValueList = [],
                StatePreservationList = processState.GetWithPredicates(new StatePreservationSearchParameterModel() { CollectionArea_CollectionAreaID = [collectionAreaID] })
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return View(collectionItemOperationParameter);
            }

            collectionItemOperationParameter.CollectionItemEntity.UsingIdentityUsersID = userManager.GetUserId(User) ?? throw new NullReferenceException();
            collectionItemOperationParameter.CollectionItemEntity.UsingIdentityUser = await userManager.GetUserAsync(User) ?? throw new NullReferenceException();
            (int statusCode, string statusMessage) = processCollectionItem.Insert(collectionItemOperationParameter);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        [HttpGet]
        [HandleStatus]
        public ActionResult Edit(int entityId)
        {
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            if (!ModelState.IsValid)
            {
                return View(collectionItemOperationParameter);
            }

            (int statusCode, string statusMessage) = processCollectionItem.Update(collectionItemOperationParameter);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        [HttpGet]
        public ActionResult Delete(int entityId)
        {
            CollectionItemOperationParameterModel? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();

            return existingCollectionItem == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" })
                : View(existingCollectionItem);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CollectionItemOperationParameterModel collectionItemOperation)
        {
            if (!ModelState.IsValid)
            {
                return View(collectionItemOperation);
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

            List<CollectionItemOperationParameterModel> modelList = [.. processCollectionItem.GetWithPredicates(collectionItemSearch)];

            MemoryStream memory = await YamlProcessor.CreateZipFile(modelList, user, hostEnvironment);

            return File(memory, "application/zip", "Download_" + user.DisplayName + ".zip");
        }

        //private void HandleStatus(Status status)
        //{
        //    if (!string.IsNullOrEmpty(status.StatusMessage))
        //    {
        //        ViewData["StatusMessage"] = stringLocalizer[status.StatusMessage];
        //        ViewData["StatusCode"] = status.StatusCode;
        //    }
        //}
    }
}