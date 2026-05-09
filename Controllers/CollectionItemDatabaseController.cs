using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CollectionItemDatabaseController(IProcessCollectionItemEntity processCollectionItem,
        IProcessCollectionArea processCollectionArea,
        IProcessStatePreservation processState,
        IProcessConcept processConcept,
        IWebHostEnvironment hostEnvironment,
        UserManager<UsingIdentityUser> userManager) : Controller
    {
        [HandleStatus]
        public ActionResult Index(CollectionItemSearchParameterModel model)
        {
            ViewData["CollectionArea"] = processCollectionArea.GetListWithPredicate(new CollectionAreaSearchParameterModel() { CollectionAreaID = model.CollectionAreaID }).FirstOrDefault();

            string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();
            model.UsingIdentityUsersID.Add(userId);

            return View(processCollectionItem.GetWithPredicates(model));
        }

        public ActionResult Create(int collectionAreaID)
        {
            List<ConceptViewModel> conceptViewModelList = [.. processConcept.Get(new ConceptualRelationshipSearchParameterModel() { CollectionAreaID = [collectionAreaID] }).Select(c => c.ConceptViewModel)];
            ViewData["ConceptWOBoolList"] = conceptViewModelList.Where(x => x.ConceptType.ToString() != "Bool").ToList();

            ViewData["StatePreservationSchemaList"] = processState.GetWithPredicates(new StatePreservationSearchParameterModel() { CollectionArea_CollectionAreaID = [collectionAreaID] })
                    .Select(s => new StatePreservationViewDTO { Id = s.StatePreservationID, Name = s.StatePreservationName }).ToList();

            CollectionItemCreateDTO createDTO = new()
            {
                CollectionAreaID = collectionAreaID,
                CollectionItemPictureList = [new()]
            };
            return View(createDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CollectionItemCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            UsingIdentityUser user = await userManager.GetUserAsync(User) ?? throw new NullReferenceException();
            (int statusCode, string statusMessage) = processCollectionItem.Insert(createDTO, user);

            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, createDTO.CollectionAreaID });
        }

        [HandleStatus]
        public ActionResult Edit(int entityId)
        {
            CollectionItemDisplayDTO? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();
            if (existingCollectionItem == null)
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" });

            List<Participant> list1 = [.. existingCollectionItem.CollectionItemNParticipantList.Select(x => x.Participant)];
            List<Place> list2 = [.. existingCollectionItem.CollectionItemNPlaceList.Select(y => y.Place)];
            ViewData["ParticipantList"] = list1;
            ViewData["PlaceList"] = list2;
            List<ConceptViewModel> conceptViewModelList = [.. processConcept.Get(new ConceptualRelationshipSearchParameterModel() { CollectionAreaID = [existingCollectionItem.CollectionItemEntity.CollectionAreaID] }).Select(c => c.ConceptViewModel)];
            ViewData["ConceptList"] = conceptViewModelList.ToList();

            ViewData["StatePreservationSchemaList"] = processState.GetWithPredicates(new StatePreservationSearchParameterModel() { CollectionArea_CollectionAreaID = [existingCollectionItem.CollectionItemEntity.CollectionAreaID] })
                    .Select(s => new StatePreservationViewDTO { Id = s.StatePreservationID, Name = s.StatePreservationName }).ToList();

            CollectionItemEditDTO editDTO = new()
            {
                Id = existingCollectionItem.CollectionItemEntity.CollectionItemEntityID,
                CollectionAreaId = existingCollectionItem.CollectionItemEntity.CollectionAreaID,
                UniqueName = existingCollectionItem.CollectionItemEntity.UniqueName,
                Fake = existingCollectionItem.CollectionItemEntity.Fake,
                Comment = existingCollectionItem.CollectionItemEntity.Comment,
                StatePreservationID = existingCollectionItem.CollectionItemEntity.StatePreservationID,
                Inscription = existingCollectionItem.CollectionItemEntity.Inscription,
                InscriptionTranslated = existingCollectionItem.CollectionItemEntity.InscriptionTranslated,
                SerialNumber = existingCollectionItem.CollectionItemEntity.SerialNumber,
                ExactYear = existingCollectionItem.CollectionItemEntity.ExactYear,
                StartYear = existingCollectionItem.CollectionItemEntity.StartYear,
                EndYear = existingCollectionItem.CollectionItemEntity.EndYear,
                IsApproximate = existingCollectionItem.CollectionItemEntity.IsApproximate,
                EraID = existingCollectionItem.CollectionItemEntity.EraID,
                EraName = existingCollectionItem.Era.EraName,
                Width = existingCollectionItem.CollectionItemEntity.Width,
                Height = existingCollectionItem.CollectionItemEntity.Height,
                Length = existingCollectionItem.CollectionItemEntity.Length,
                Diameter = existingCollectionItem.CollectionItemEntity.Diameter,
                Weight = existingCollectionItem.CollectionItemEntity.Weight,
                PersonalIdentificationNumber = existingCollectionItem.CollectionItemEntity.PersonalIdentificationNumber,
                FilingLocation = existingCollectionItem.CollectionItemEntity.FilingLocation,
                DeliveryPrice = existingCollectionItem.CollectionItemEntity.DeliveryPrice,
                DeliveryDate = existingCollectionItem.CollectionItemEntity.DeliveryDate,
                DeliveryAdress = existingCollectionItem.CollectionItemEntity.DeliveryAdress,
                IsCollectionItemPublic = existingCollectionItem.CollectionItemEntity.IsCollectionItemPublic,
                CollectionItemPictureList = [.. existingCollectionItem.CollectionItemPictureList.Select(x => new PictureToCollectionItemEditDTO
                {
                    Id = x.CollectionItemPictureID,
                    PerspectiveInt = (int)x.PerspectiveInt
                })],
                ConceptValueList = [.. existingCollectionItem.ConceptValueList.Select(x => new ConceptValueToCollectionItemEditDTO {
                    ConceptId = x.ConceptID,
                    ConceptValueId = x.ConceptValueID,
                    ValueString = x.ValueString,
                    ValueInt = x.ValueInt,
                    ValueDecimal = x.ValueDecimal,
                    ValueDate = x.ValueDate,
                })],
                ConnectedParticipantList = [.. existingCollectionItem.CollectionItemNParticipantList.Select(x => new ParticipantToCollectionItemCreateDTO
                {
                    Id = x.ParticipantID,
                    Relationship = x.RelationType.CollectionItemRelationshipName
                })],
                ConnectedPlaceList = [.. existingCollectionItem.CollectionItemNPlaceList.Select(x => new PlaceToCollectionItemCreateDTO
                {
                    Id = x.PlaceID,
                    Relationship = x.RelationType.CollectionItemRelationshipName
                })],
            };

            return View(editDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CollectionItemEditDTO editDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(editDTO);
            }

            (int statusCode, string statusMessage) = processCollectionItem.Update(editDTO);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, editDTO.Id });
        }

        public ActionResult Delete(int entityId)
        {
            CollectionItemDisplayDTO? existingCollectionItem = processCollectionItem
                .GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] })
                .FirstOrDefault();

            return existingCollectionItem == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionItemEntity_NotFound" })
                : View(existingCollectionItem);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int collectionItemEntityID, int collectionAreaID)
        {
            if (collectionItemEntityID <= 0 || collectionAreaID <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });


            (int statusCode, string statusMessage) = processCollectionItem.Delete(collectionItemEntityID);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage, collectionAreaID });
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

            List<CollectionItemDisplayDTO> modelList = [.. processCollectionItem.GetWithPredicates(collectionItemSearch)];

            MemoryStream memory = await YamlProcessor.CreateZipFile(modelList, user, hostEnvironment);

            return File(memory, "application/zip", "Download_" + user.DisplayName + ".zip");
        }
    }
}