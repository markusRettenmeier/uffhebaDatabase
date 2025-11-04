using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.Processes.CollectionAreaProcesses;
using Sammlerplattform.Services.Processes.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    public class CollectionItemDatabaseController(IProcessCollectionItemEntity processCollectionItem,
        IProcessCollectionArea processCollectionArea,
        IProcessCollectionAttribute processCollectionAttribute,
        IProcessState processState,
        UserManager<UsingIdentityUser> userManager,
        IWebHostEnvironment hostEnvironment) : Controller
    {
        [Authorize]
        public ActionResult Index(string statusMessage, CollectionItemSearchParameterModel model)
        {
            ViewData["StatusMessage"] = statusMessage;
            ViewData["CollectionArea"] = processCollectionArea.GetListWithPredicate(new CollectionAreaSearchParameterModel() { CollectionAreaID = model.CollectionAreaID }).FirstOrDefault();

            string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();
            model.UsingIdentityUsersID.Add(userId);

            return View(processCollectionItem.GetWithPredicates(model));
        }

        public ActionResult Create(string statusMessage, int collectionAreaID)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionItemOperationParameterModel model = new()
            {
                CollectionItemEntity = new()
                {
                    UsingIdentityUsersID = string.Empty, // wird in CreateSubmit gesetzt
                    CollectionAreaID = collectionAreaID
                },
                CollectionItemPictureList =
                    [
                        new CollectionItemPicture() // MINDESTENS EIN ELEMENT HINZUFÜGEN, weil CollectionItemPictureList[0] in Create
                    ],
                CollectionItemValueList = [],
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

        public ActionResult Edit(string statusMessage, int entityId)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionItemSearchParameterModel entitySearch = new();
            entitySearch.CollectionItemEntityID.Add(entityId);
            CollectionItemOperationParameterModel? model = processCollectionItem.GetWithPredicates(entitySearch).FirstOrDefault();

            return View(model);
        }
        public ActionResult EditSubmit(CollectionItemOperationParameterModel collectionItemOperationParameter)
        {
            string statusMessage = processCollectionItem.Update(collectionItemOperationParameter);

            return RedirectToAction(nameof(Index), new { statusMessage, collectionItemOperationParameter.CollectionItemEntity.CollectionAreaID });
        }

        public ActionResult Delete(string statusMessage, int entityId)
        {
            ViewData["StatusMessage"] = statusMessage;

            CollectionItemSearchParameterModel collectionItemSearch = new();
            collectionItemSearch.CollectionItemEntityID.Add(entityId);
            return View(processCollectionItem.GetWithPredicates(collectionItemSearch).FirstOrDefault());
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
                return RedirectToAction(nameof(Index), new { statusMessage = "User wurde nicht gefunden." });
            }

            CollectionItemSearchParameterModel collectionItemSearch = new();
            if (entityId > 0)
            {
                collectionItemSearch.CollectionItemEntityID.Add((int)entityId);
            }
            collectionItemSearch.UsingIdentityUsersID.Add(user.Id);
            List<CollectionItemOperationParameterModel> modelList = [.. processCollectionItem.GetWithPredicates(collectionItemSearch)];
            MemoryStream memory = await YamlProcessor.CreateZipFile(modelList, user, hostEnvironment);

            return File(memory, "application/zip", "Download_" + user.UserName + ".zip");
        }
    }
}
