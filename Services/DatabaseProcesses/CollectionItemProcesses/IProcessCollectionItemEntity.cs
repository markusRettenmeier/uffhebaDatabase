using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase.IndustryDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.Translation;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEntity
    {
        List<CollectionItemDisplayDTO> GetWithTranslationsListViaPredicates(CollectionItemSearchParameterModel model, int? topK = null);
        List<CollectionItemEntity> GetEntityListViaPredicates(CollectionItemSearchParameterModel model, int? topK = null);
        Task<List<CollectionItemDisplayDTO>> GetWithVector(CollectionItemSearchParameterModel model, int? topK = null);
        (int statusCode, string statusMessage) Insert(CollectionItemCreateDTO createDto, UsingIdentityUser user);
        (int statusCode, string statusMessage) Update(CollectionItemEditDTO editDTO);
        (int statusCode, string statusMessage) Delete(int id);
    }

    public class CollectionItemEntityProcessor(IUnitOfWork unitOfWork,
        IProcessCollectionItemPicture processCollectionItemPicture,
        IProcessPicturePhysically processPicturePhysically,
        IProcessConceptValue processConceptValue,
        IProcessCollectionItemEmbedding processCollectionItemEmbedding,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITrackEventsText trackEvents,
        IProcessCIRelationship processRelationship) : IProcessCollectionItemEntity
    {
        public (int statusCode, string statusMessage) Insert(CollectionItemCreateDTO createDTO, UsingIdentityUser user)
        {
            List<(PictureToCollectionItemCreateDTO, int)> collectionItemPictureList = [];

            try
            {
                //hier using verwenden, da sonst bei Fehlern in der Mitte des Prozesses teilweise Daten in der DB sind, die nicht mehr mit den physischen Bildern übereinstimmen oder teilweise Verbindungen zu anderen Entities bestehen, die nicht mehr existieren
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionItemEntity newCollectionItemEntity = new()
                {
                    CollectionAreaID = createDTO.CollectionAreaID,
                    Fake = createDTO.Fake,
                    StatePreservationID = createDTO.StatePreservationID,
                    Inscription = createDTO.Inscription,
                    SerialNumber = createDTO.SerialNumber,
                    ExactYear = createDTO.ExactYear,
                    StartYear = createDTO.StartYear,
                    EndYear = createDTO.EndYear,
                    IsApproximate = createDTO.IsApproximate,
                    EraID = createDTO.EraID,
                    Width = createDTO.Width,
                    Height = createDTO.Height,
                    Length = createDTO.Length,
                    Diameter = createDTO.Diameter,
                    Weight = createDTO.Weight,
                    PersonalIdentificationNumber = createDTO.PersonalIdentificationNumber,
                    FilingLocation = createDTO.FilingLocation,
                    DeliveryAdress = createDTO.DeliveryAdress,
                    DeliveryPrice = createDTO.DeliveryPrice,
                    DeliveryDate = createDTO.DeliveryDate,
                    UsingIdentityUsersID = user.Id,
                    IsCollectionItemPublic = createDTO.IsCollectionItemPublic
                };
                newCollectionItemEntity = unitOfWork.CollectionItemEntityRepository.Insert(newCollectionItemEntity);
                unitOfWork.Save();

                Dictionary<string, string> translationList = []; //For Vector Search
                if (!string.IsNullOrEmpty(createDTO.Comment))
                {
                    translationList.Add(nameof(CollectionItemDisplayDTO.Comment), string.Join(", ", processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Comment,
                        EntityName = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        PropertyName = nameof(CollectionItemDisplayDTO.Comment)
                    })));
                }
                if (!string.IsNullOrEmpty(createDTO.Inscription))
                {
                    translationList.Add(nameof(CollectionItemDisplayDTO.Inscription), string.Join(", ", processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Inscription,
                        EntityName = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        PropertyName = nameof(CollectionItemDisplayDTO.Inscription)
                    })));
                }
                if (!string.IsNullOrEmpty(createDTO.UniqueName))
                {
                    translationList.Add(nameof(CollectionItemDisplayDTO.UniqueName), string.Join(", ", processTranslations.Insert(
                        new TranslationDTO
                        {
                            TextToTranslate = createDTO.UniqueName,
                            EntityName = nameof(CollectionItemEntity),
                            EntityId = newCollectionItemEntity.CollectionItemEntityID,
                            PropertyName = nameof(CollectionItemDisplayDTO.UniqueName)
                        })));
                }

                foreach (ParticipantToCollectionItemCreateDTO createDto in createDTO.ConnectedParticipantList)
                {
                    translationList.Add(nameof(Participant.ParticipantName), ConnectParticipantToCollectionItemEntity(newCollectionItemEntity, createDto.Id, createDto.Relationship));
                }
                foreach (PlaceToCollectionItemCreateDTO place in createDTO.ConnectedPlaceList)
                {
                    translationList.Add(nameof(Toponymy.ToponymyName), string.Join(", ", ConnectPlaceToCollectionItemEntity(newCollectionItemEntity, place.Id, place.Relationship)));
                }
                foreach (ConceptValueToCollectionItemCreateDTO coneptValue in createDTO.ConceptValueList)
                {
                    ConceptValue newConceptValue = new()
                    {
                        ConceptID = coneptValue.ConceptId,
                        ValueString = coneptValue.ValueString,
                        ValueInt = coneptValue.ValueInt,
                        ValueDecimal = coneptValue.ValueDecimal,
                        ValueDate = coneptValue.ValueDate
                    };
                    List<string> coneptValueTranslationList = processConceptValue.Insert(newConceptValue, newCollectionItemEntity.CollectionItemEntityID);
                    translationList.Add(nameof(ConceptValue.ValueString), string.Join(", ", coneptValueTranslationList));
                }

                CollectionItemDisplayDTO displayDTO = EntityToDiplayDTO(newCollectionItemEntity);
                (int statuscode, string statusmessage) = processCollectionItemEmbedding.Insert(displayDTO, translationList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }

                foreach (PictureToCollectionItemCreateDTO collectionItemPicture in createDTO.CollectionItemPictureList)
                {
                    CollectionItemPicture newCollectionItemPicture = new()
                    {
                        CollectionItemEntityID = newCollectionItemEntity.CollectionItemEntityID,
                        IFormFile = collectionItemPicture.IFormFile,
                        PerspectiveInt = collectionItemPicture.PerspectiveInt
                    };
                    (int code, string returnMessage, int pictureId) = processCollectionItemPicture.Insert(newCollectionItemPicture);
                    collectionItemPictureList.Add((collectionItemPicture, pictureId));
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (code, returnMessage);
                    }
                }
                foreach ((PictureToCollectionItemCreateDTO, int) picture in collectionItemPictureList)
                {
                    (int code, string returnMessage) = processPicturePhysically.SaveCollectionItemPic(picture.Item1, picture.Item2, user.DisplayName);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (code, returnMessage);
                    }
                }

                scope.Complete();

                return (200, "Success_CollectionItemEntity_Created");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "InsertCollectionItemEntity_Failed_Exception", new Dictionary<string, object>
                {
                    { "CollectionItemEntityCreateDTO", createDTO }
                });
                return (500, "Error_Unknown");
            }
        }

        public (int statusCode, string statusMessage) Update(CollectionItemEditDTO editDto)
        {
            CollectionItemSearchParameterModel collectionItemSearchParameterModel = new();
            collectionItemSearchParameterModel.CollectionItemEntityID.Add(editDto.Id);
            CollectionItemEntity? existingEntity = GetEntityListViaPredicates(collectionItemSearchParameterModel).FirstOrDefault();
            if (existingEntity == null)
            {
                trackEvents.TrackError("UpdateCollectionItemEntity_Failed_NotFound", new Dictionary<string, object>
                {
                    { "CollectionItemEntityID", editDto.Id.ToString() }
                });
                return (400, "Error_CollectionItemEntity_NotFound");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Dictionary<string, string> translationList = [];
                int statuscode = 0;
                string statusmessage = "";

                foreach (var kvp in ChangeEntity(editDto, existingEntity))
                    translationList.TryAdd(kvp.Key, kvp.Value);
                foreach (var kvp in SyncParticipantConnections(existingEntity, editDto.ConnectedParticipantList))
                    translationList.TryAdd(kvp.Key, kvp.Value);
                foreach (var kvp in SyncPlaceConnections(existingEntity, editDto.ConnectedPlaceList))
                    translationList.TryAdd(kvp.Key, kvp.Value);
                List<ConceptValue> conceptValueList = [.. editDto.ConceptValueList.Select(x => new ConceptValue
                {
                    ConceptValueID = x.ConceptValueId,
                    ConceptID = x.ConceptId,
                    ValueString = x.ValueString,
                    ValueInt = x.ValueInt,
                    ValueDecimal = x.ValueDecimal,
                    ValueDate = x.ValueDate,
                    ValueBool = x.ValueBool
                })];
                translationList.Add(nameof(ConceptValue.ValueString), string.Join(", ", SyncConceptValueConnections(existingEntity, conceptValueList)));
                CollectionItemDisplayDTO displayDTO = EntityToDiplayDTO(existingEntity);
                processCollectionItemEmbedding.Update(displayDTO, translationList);

                (List<(PictureToCollectionItemEditDTO collectionItemPicture, int pictureId, string process)> collectionItemPictureList, statuscode, statusmessage) = SyncCollectionItemPictureConnections(existingEntity, editDto.CollectionItemPictureList, editDto.DeletedPictureIds);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }
                foreach (var (collectionItemPicture, pictureId, process) in collectionItemPictureList.Where(x => x.process == "insert"))
                {
                    PictureToCollectionItemCreateDTO pictureToCollectionItemCreateDTO = new()
                    {
                        IFormFile = collectionItemPicture.IFormFile,
                        PerspectiveInt = collectionItemPicture.PerspectiveInt
                    };
                    (statuscode, statusmessage) = processPicturePhysically.SaveCollectionItemPic(pictureToCollectionItemCreateDTO, pictureId, existingEntity.UsingIdentityUser.DisplayName);
                    if (statuscode != 200)
                    {
                        foreach (var pic in collectionItemPictureList)
                        {
                            processPicturePhysically.DeleteCollectionItemPic(pic.pictureId);
                        }
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in collectionItemPictureList.Where(x => x.process == "delete"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.DeleteCollectionItemPic(pictureId);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }

                scope.Complete();

                return (200, "Success_CollectionItemEntity_Changed");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "UpdateCollectionItemEntity_Failed_Exception",
                    new Dictionary<string, object>
                    {
                        { "CollectionItemEntity", editDto }
                    });
                return (500, "Error_Unknown");
            }

            Dictionary<string, string> ChangeEntity(CollectionItemEditDTO editDto, CollectionItemEntity existingEntity)
            {
                bool hasChanges = false;
                Dictionary<string, string> translationList = [];

                var existingTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    EntityName = [nameof(CollectionItemEntity)],
                    EntityId = [existingEntity.CollectionItemEntityID]
                });

                string? translatedComment = existingTranslations.FirstOrDefault(x => x.PropertyName == nameof(CollectionItemDisplayDTO.Comment))?.TranslatedText;
                if (!string.IsNullOrEmpty(editDto.Comment))
                {
                    if (translatedComment != null)
                    {
                        if (translatedComment != editDto.Comment)
                        {
                            translationList.Add(nameof(CollectionItemDisplayDTO.Comment), string.Join(", ", processTranslations.Update(
                                new TranslationDTO
                                {
                                    TextToTranslate = editDto.Comment,
                                    EntityName = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    PropertyName = nameof(CollectionItemDisplayDTO.Comment)
                                })));
                        }
                    }
                    else
                    {
                        translationList.Add(nameof(CollectionItemDisplayDTO.Comment), string.Join(", ", processTranslations.Insert(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.Comment,
                                EntityName = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                PropertyName = nameof(CollectionItemDisplayDTO.Comment)
                            })));
                    }
                }
                else if (translatedComment != null)
                {
                    processTranslations.Delete(new EntityTranslationSearchParameter
                    {
                        EntityName = [nameof(CollectionItemEntity)],
                        PropertyName = [nameof(CollectionItemDisplayDTO.Comment)],
                        EntityId = [existingEntity.CollectionItemEntityID]
                    });
                }
                if (existingEntity.Width != editDto.Width)
                {
                    existingEntity.Width = editDto.Width;
                    hasChanges = true;
                }
                if (existingEntity.Height != editDto.Height)
                {
                    existingEntity.Height = editDto.Height;
                    hasChanges = true;
                }
                if (existingEntity.Length != editDto.Length)
                {
                    existingEntity.Length = editDto.Length;
                    hasChanges = true;
                }
                if (existingEntity.Diameter != editDto.Diameter)
                {
                    existingEntity.Diameter = editDto.Diameter;
                    hasChanges = true;
                }
                if (existingEntity.Weight != editDto.Weight)
                {
                    existingEntity.Weight = editDto.Weight;
                    hasChanges = true;
                }
                if (existingEntity.StatePreservationID != editDto.StatePreservationID)
                {
                    existingEntity.StatePreservationID = editDto.StatePreservationID;
                    hasChanges = true;
                }
                if (existingEntity.Fake != editDto.Fake)
                {
                    existingEntity.Fake = editDto.Fake;
                    hasChanges = true;
                }
                if (existingEntity.FilingLocation != editDto.FilingLocation)
                {
                    existingEntity.FilingLocation = editDto.FilingLocation;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryPrice != editDto.DeliveryPrice)
                {
                    existingEntity.DeliveryPrice = editDto.DeliveryPrice;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryDate != editDto.DeliveryDate)
                {
                    existingEntity.DeliveryDate = editDto.DeliveryDate;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryAdress != editDto.DeliveryAdress)
                {
                    existingEntity.DeliveryAdress = editDto.DeliveryAdress;
                    hasChanges = true;
                }
                if (existingEntity.StartYear != editDto.StartYear)
                {
                    existingEntity.StartYear = editDto.StartYear;
                    hasChanges = true;
                }
                if (existingEntity.EndYear != editDto.EndYear)
                {
                    existingEntity.EndYear = editDto.EndYear;
                    hasChanges = true;
                }
                if (existingEntity.ExactYear != editDto.ExactYear)
                {
                    existingEntity.ExactYear = editDto.ExactYear;
                    hasChanges = true;
                }
                if (existingEntity.IsApproximate != editDto.IsApproximate)
                {
                    existingEntity.IsApproximate = editDto.IsApproximate;
                    hasChanges = true;
                }

                string? translatedUniqueName = existingTranslations.FirstOrDefault(x => x.PropertyName == nameof(CollectionItemDisplayDTO.UniqueName))?.TranslatedText;
                if (!string.IsNullOrEmpty(editDto.UniqueName))
                {
                    if (translatedUniqueName != null)
                    {
                        if (translatedUniqueName != editDto.UniqueName)
                        {
                            translationList.Add(nameof(CollectionItemDisplayDTO.UniqueName), string.Join(", ", processTranslations.Update(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.UniqueName,
                                EntityName = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                PropertyName = nameof(CollectionItemDisplayDTO.UniqueName)
                            })));
                        }
                    }
                    else
                    {
                        translationList.Add(nameof(CollectionItemDisplayDTO.UniqueName), string.Join(", ", processTranslations.Insert(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.UniqueName,
                                EntityName = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                PropertyName = nameof(CollectionItemDisplayDTO.UniqueName)
                            })));
                    }
                }
                else if (translatedUniqueName != null)
                {
                    processTranslations.Delete(
                        new EntityTranslationSearchParameter
                        {
                            EntityName = [nameof(CollectionItemEntity)],
                            PropertyName = [nameof(CollectionItemDisplayDTO.UniqueName)],
                            EntityId = [existingEntity.CollectionItemEntityID]
                        });
                }

                if (existingEntity.Inscription != editDto.Inscription)
                {
                    existingEntity.Inscription = editDto.Inscription;
                    hasChanges = true;

                    if (!string.IsNullOrEmpty(editDto.Inscription))
                    {
                        string? translatedInscription = existingTranslations.FirstOrDefault(x => x.PropertyName == nameof(CollectionItemDisplayDTO.InscriptionTranslated))?.TranslatedText;
                        if (translatedInscription != null)
                        {
                            if (translatedInscription != editDto.InscriptionTranslated)
                            {
                                translationList.Add(nameof(CollectionItemDisplayDTO.Inscription), string.Join(", ", processTranslations.Update(
                                    new TranslationDTO
                                    {
                                        TextToTranslate = editDto.Inscription,
                                        EntityName = nameof(CollectionItemEntity),
                                        EntityId = existingEntity.CollectionItemEntityID,
                                        PropertyName = nameof(CollectionItemDisplayDTO.Inscription)
                                    })));
                            }
                        }
                        else
                        {
                            translationList.Add(nameof(CollectionItemDisplayDTO.Inscription), string.Join(", ", processTranslations.Insert(
                                new TranslationDTO
                                {
                                    TextToTranslate = editDto.Inscription,
                                    EntityName = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    PropertyName = nameof(CollectionItemDisplayDTO.Inscription)
                                })));
                        }
                    }
                    else
                    {
                        processTranslations.Delete(new EntityTranslationSearchParameter
                        {
                            EntityName = [nameof(CollectionItemEntity)],
                            PropertyName = [nameof(CollectionItemDisplayDTO.InscriptionTranslated)],
                            EntityId = [existingEntity.CollectionItemEntityID],
                        });
                    }
                }
                if (existingEntity.PersonalIdentificationNumber != editDto.PersonalIdentificationNumber)
                {
                    existingEntity.PersonalIdentificationNumber = editDto.PersonalIdentificationNumber;
                    hasChanges = true;
                }
                if (existingEntity.EraID != editDto.EraID)
                {
                    existingEntity.EraID = editDto.EraID;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    unitOfWork.Save();
                }
                return translationList;
            }
        }

        private static CollectionItemDisplayDTO EntityToDiplayDTO(CollectionItemEntity existingEntity)
        {
            return new()
            {
                CollectionItemEntityID = existingEntity.CollectionItemEntityID,
                CollectionAreaID = existingEntity.CollectionAreaID,
                Fake = existingEntity.Fake,
                StatePreservationID = existingEntity.StatePreservationID,
                Inscription = existingEntity.Inscription,
                SerialNumber = existingEntity.SerialNumber,
                ExactYear = existingEntity.ExactYear,
                StartYear = existingEntity.StartYear,
                EndYear = existingEntity.EndYear,
                IsApproximate = existingEntity.IsApproximate,
                EraID = existingEntity.EraID,
                Width = existingEntity.Width,
                Height = existingEntity.Height,
                Length = existingEntity.Length,
                Diameter = existingEntity.Diameter,
                Weight = existingEntity.Weight,
                PersonalIdentificationNumber = existingEntity.PersonalIdentificationNumber,
                FilingLocation = existingEntity.FilingLocation,
                DeliveryAdress = existingEntity.DeliveryAdress,
                DeliveryPrice = existingEntity.DeliveryPrice,
                DeliveryDate = existingEntity.DeliveryDate
            };
        }

        public (int statusCode, string statusMessage) Delete(int id)
        {
            CollectionItemEntity? existingCollectionItem = GetEntityListViaPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [id] }).FirstOrDefault();
            if (existingCollectionItem == null)
            {
                return (400, "Error_CollectionItemEntity_NotFound");
            }
            List<int> collectionItemPictureList = [];

            try
            {
                using TransactionScope scope = new();

                for (int i = existingCollectionItem.CollectionItemNParticipantList.Count - 1; i == 0; i--)
                {
                    DisconnectParticipantConnection(existingCollectionItem, existingCollectionItem.CollectionItemNParticipantList[i].ParticipantID);
                }
                for (int i = existingCollectionItem.CollectionItemNPlaceList.Count - 1; i == 0; i--)
                {
                    DisconnectPlaceConnection(existingCollectionItem, existingCollectionItem.CollectionItemNPlaceList[i].PlaceID);
                }
                for (int i = existingCollectionItem.CollectionItemPictureList.Count - 1; i == 0; i--)
                {
                    collectionItemPictureList.Add(existingCollectionItem.CollectionItemPictureList[i].CollectionItemPictureID);
                    (int statuscode, string statusmessage) = processCollectionItemPicture.Delete(existingCollectionItem.CollectionItemPictureList[i]);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                for (int i = existingCollectionItem.ConceptValueList.Count - 1; i == 0; i--)
                {
                    processConceptValue.Delete(existingCollectionItem.ConceptValueList[i].ConceptValueID);
                }
                processCollectionItemEmbedding.Delete(id);

                unitOfWork.CollectionItemEntityRepository.Delete(existingCollectionItem);
                unitOfWork.Save();

                foreach (int pictureID in collectionItemPictureList)
                {
                    (int statuscode, string statusmessage) = processPicturePhysically.DeleteCollectionItemPic(pictureID);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }

                scope.Complete();

                return (200, "Success_CollectionItemEntity_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "DeleteCollectionItemEntity_Failed_Exception", new Dictionary<string, object>
                {
                    { "CollectionItemEntityId", id }
                });
                return (500, "Error_Unknown");
            }
        }

        public List<CollectionItemDisplayDTO> GetWithTranslationsListViaPredicates(CollectionItemSearchParameterModel model, int? topK = null)
        {
            IQueryable<CollectionItemDisplayDTO> collectionItemIQueryable = unitOfWork.CollectionItemEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemEntity>(model),
                includeProperties: GetIncludeProperties())
                .Select(b => SetMembersofEntity(b));
            collectionItemIQueryable = collectionItemIQueryable.AsNoTracking();
            if (topK != null)
                collectionItemIQueryable = collectionItemIQueryable.Take(topK.Value);

            List<CollectionItemDisplayDTO> collectionItemList = SetTranslations([.. collectionItemIQueryable]);

            return collectionItemList;
        }

        public List<CollectionItemEntity> GetEntityListViaPredicates(CollectionItemSearchParameterModel model, int? topK = null)
        {
            IQueryable<CollectionItemEntity> collectionItemIQueryable = unitOfWork.CollectionItemEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemEntity>(model),
                includeProperties: GetIncludeProperties());
            collectionItemIQueryable = collectionItemIQueryable.AsNoTracking();
            return [.. collectionItemIQueryable];
        }
        private static string GetIncludeProperties()
        {
            return nameof(CollectionItemEntity.CollectionItemPictureList) + "," +
                   nameof(CollectionItemEntity.UsingIdentityUser) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPlaceList) + "." + nameof(CollectionItemNPlace.Place) +
                       "." + nameof(Place.PlaceNToponymyList) + "." + nameof(PlaceNToponymy.Toponymy) + "," +
                   nameof(CollectionItemEntity.CollectionItemNParticipantList) + "." + nameof(CollectionItemNParticipant.Participant) + "," +
                   nameof(CollectionItemEntity.ConceptValueList);
        }
        private static CollectionItemDisplayDTO SetMembersofEntity(CollectionItemEntity b)
        {
            return new CollectionItemDisplayDTO
            {
                CollectionItemEntityID = b.CollectionItemEntityID,
                CollectionAreaID = b.CollectionAreaID,
                Fake = b.Fake,
                StatePreservationID = b.StatePreservationID,
                Inscription = b.Inscription,
                SerialNumber = b.SerialNumber,
                ExactYear = b.ExactYear,
                StartYear = b.StartYear,
                EndYear = b.EndYear,
                IsApproximate = b.IsApproximate,
                EraID = b.EraID,
                Width = b.Width,
                Height = b.Height,
                Length = b.Length,
                Diameter = b.Diameter,
                Weight = b.Weight,
                PersonalIdentificationNumber = b.PersonalIdentificationNumber,
                FilingLocation = b.FilingLocation,
                DeliveryAdress = b.DeliveryAdress,
                DeliveryPrice = b.DeliveryPrice,
                DeliveryDate = b.DeliveryDate,
                CollectionItemPictureList = b.CollectionItemPictureList,
                ConceptValueList = b.ConceptValueList,
                CollectionItemNParticipantList = [.. b.CollectionItemNParticipantList.Select(p => new CollectionItemNParticipantDisplayDTO
                {
                    ParticipantID = p.ParticipantID,
                    Name = p.Participant?.ParticipantName ?? string.Empty,
                    RelationshipId = p.RelationTypeId
                })],
                CollectionItemNPlaceList = [.. b.CollectionItemNPlaceList.Select(p => new CollectionItemNPlaceDisplayDTO
                {
                    PlaceID = p.PlaceID,
                    ToponymyList = p.Place?.PlaceNToponymyList.Select(pt => new ToponymyDisplayDTO
                    {
                        Id = pt.ToponymyID,
                        Name = pt.Toponymy?.ToponymyName ?? string.Empty
                    }).ToList() ?? [],
                    RelationshipId = p.RelationTypeId
                })]
            };
        }
        private List<CollectionItemDisplayDTO> SetTranslations(List<CollectionItemDisplayDTO> operationParameterList)
        {
            var culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name);
            var allTranslations = unitOfWork.EntityTranslationRepository
                .Get(filter: x => x.Culture == culture)
                .ToList();

            // Dictionary für schnellen Zugriff
            var translationDict = allTranslations
                .GroupBy(x => (x.EntityName, x.EntityId, x.PropertyName))
                .ToDictionary(
                    g => g.Key,
                    g => g.First().TranslatedText
                );

            foreach (var ciop in operationParameterList)
            {
                // CollectionArea
                var areaKey = (nameof(CollectionArea), ciop.CollectionAreaID, nameof(CollectionItemDisplayDTO.CollectionAreaName));
                ciop.CollectionAreaName = translationDict.TryGetValue(areaKey, out var value) ? value : string.Empty;

                // CollectionItemEntity Übersetzungen
                var entityId = ciop.CollectionItemEntityID;

                var uniqueNameKey = (nameof(CollectionItemEntity), entityId, nameof(CollectionItemDisplayDTO.UniqueName));
                ciop.UniqueName = translationDict.TryGetValue(uniqueNameKey, out var uniqueName) ? uniqueName : string.Empty;

                var inscriptionKey = (nameof(CollectionItemEntity), entityId, nameof(CollectionItemDisplayDTO.InscriptionTranslated));
                ciop.InscriptionTranslated = translationDict.TryGetValue(inscriptionKey, out var inscription) ? inscription : string.Empty;

                var commentKey = (nameof(CollectionItemEntity), entityId, nameof(CollectionItemDisplayDTO.Comment));
                ciop.Comment = translationDict.TryGetValue(commentKey, out var comment) ? comment : string.Empty;

                // Era
                if (ciop.EraID > 0)
                {
                    var eraKey = (nameof(Era), ciop.EraID.Value, nameof(EraDisplayDTO.EraName));
                    ciop.EraName = translationDict.TryGetValue(eraKey, out var eraName) ? eraName : string.Empty;
                }

                // StatePreservation
                if (ciop.StatePreservationID > 0)
                {
                    var stateKey = (nameof(StatePreservation), ciop.StatePreservationID.Value, nameof(CollectionItemDisplayDTO.StatePreservationName));
                    ciop.StatePreservationName = translationDict.TryGetValue(stateKey, out var stateName) ? stateName : string.Empty;
                }

                // ConceptValueList
                foreach (var cav in ciop.ConceptValueList)
                {
                    var cvKey = (nameof(ConceptValue), cav.ConceptValueID, nameof(ConceptValue.ValueString));
                    cav.ValueString = translationDict.TryGetValue(cvKey, out var cvValue) ? cvValue : string.Empty;

                    var conceptKey = (nameof(Concept), cav.ConceptID, nameof(ConceptViewModel.Name));
                    cav.ConceptName = translationDict.TryGetValue(conceptKey, out var conceptName) ? conceptName : string.Empty;
                }

                // Places
                foreach (var placeConnection in ciop.CollectionItemNPlaceList)
                {
                    var placeKey = (nameof(CollectionItemRelationship), placeConnection.RelationshipId, nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName));
                    placeConnection.RelationshipName = translationDict.TryGetValue(placeKey, out var placeName) ? placeName : string.Empty;
                }

                // Participants
                foreach (var participantConnection in ciop.CollectionItemNParticipantList)
                {
                    var participantKey = (nameof(CollectionItemRelationship), participantConnection.RelationshipId, nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName));
                    participantConnection.RelationshipName = translationDict.TryGetValue(participantKey, out var participantName) ? participantName : string.Empty;
                    if (participantConnection.IndustryId != null)
                    {
                        var industryKey = (nameof(Industry), participantConnection.IndustryId.Value, nameof(CollectionItemNParticipantDisplayDTO.IndustryName));
                        participantConnection.IndustryName = translationDict.TryGetValue(industryKey, out var industryName) ? industryName : string.Empty;
                    }
                }
            }

            return operationParameterList;
        }

        public async Task<List<CollectionItemDisplayDTO>> GetWithVector(CollectionItemSearchParameterModel model, int? topK = null)
        {
            if (string.IsNullOrEmpty(model.SemanticSearchQuery))
                return GetWithTranslationsListViaPredicates(model, topK);

            var vectorResults = await processCollectionItemEmbedding.SearchAsync(model.SemanticSearchQuery);
            if (vectorResults.Count == 0)
            {
                return [];
            }

            model.CollectionItemEntityID = [.. vectorResults.Select(x => x.CollectionItemEntityID)];
            return GetWithTranslationsListViaPredicates(model, topK);
        }

        /// <summary>
        /// Connects a participant to a collection item entity with a specific relationship. Returns the translation of the connected participant.
        /// </summary>
        /// <param name="collectionItemEntity"></param>
        /// <param name="participantID"></param>
        /// <param name="relationship"></param>
        /// <returns></returns>

        private string ConnectParticipantToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int participantID, string relationship)
        {
            if (participantID <= 0)
            {
                return string.Empty;
            }
            Participant? participant = unitOfWork.ParticipantRepository.GetByID(participantID);
            if (participant is null)
            {
                return string.Empty;
            }

            List<CollectionItemRelationship> relationshipList = processRelationship.GetEntityListViaPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return string.Empty;
            }

            CollectionItemNParticipant collectionItemEntityNParticipant = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                ParticipantID = participantID,
                RelationTypeId = relationshipList.First().CollectionItemRelationshipId
            };
            _ = unitOfWork.CollectionItemNParticipantRepository.Insert(collectionItemEntityNParticipant);
            unitOfWork.Save();

            return participant.ParticipantName;
        }
        private Dictionary<string, string> SyncParticipantConnections(CollectionItemEntity existingCollectionItemEntity, List<ParticipantToCollectionItemCreateDTO> newConnections)
        {
            List<CollectionItemNParticipant> currentConnections = existingCollectionItemEntity.CollectionItemNParticipantList;

            for (int i = currentConnections.Count - 1; i == 0; i--)
            {
                ParticipantToCollectionItemCreateDTO? updated = newConnections.FirstOrDefault(x => x.Id == currentConnections[i].ParticipantID);

                if (updated == null)
                {
                    DisconnectParticipantConnection(existingCollectionItemEntity, currentConnections[i].ParticipantID);
                }
                else
                {
                    UpdateCollectionItemNParticipant(existingCollectionItemEntity, updated);
                }
            }

            Dictionary<string, string> translationList = [];
            foreach (ParticipantToCollectionItemCreateDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ParticipantID == newItem.Id);
                if (!exists)
                {
                    translationList.Add(nameof(Participant.ParticipantName), ConnectParticipantToCollectionItemEntity(existingCollectionItemEntity, newItem.Id, newItem.Relationship));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNParticipant(CollectionItemEntity existingCollectionItemEntity, ParticipantToCollectionItemCreateDTO updated)
        {
            List<CollectionItemRelationship> relationshipList = processRelationship.GetEntityListViaPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [updated.Relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return;
            }

            CIRelationshipDisplayDTO? relationship = processRelationship.GetWithTranslationsListViaPredicates(new CIRelationshipSearchParameterModel { CollectionItemRelationshipName = [updated.Relationship] }).FirstOrDefault();
            CollectionItemNParticipant? collectionItemNParticipant = (from bep in unitOfWork.CollectionItemNParticipantRepository.Get(includeProperties: nameof(Participant))
                                                                      where bep.ParticipantID == updated.Id && bep.CollectionItemEntity == existingCollectionItemEntity
                                                                      select bep).FirstOrDefault();
            if (collectionItemNParticipant != null && relationship != null)
            {
                if (relationship.Id != collectionItemNParticipant.RelationTypeId)
                {
                    collectionItemNParticipant.RelationTypeId = relationship.Id;
                    unitOfWork.Save();
                }
            }
        }
        private void DisconnectParticipantConnection(CollectionItemEntity collectionItemEntity, int personID)
        {
            if (collectionItemEntity.CollectionItemEntityID > 0 && personID > 0)
            {
                CollectionItemNParticipant? collectionItemNParticipant = (from bep in unitOfWork.CollectionItemNParticipantRepository.Get(includeProperties: nameof(Participant))
                                                                          where bep.ParticipantID == personID && bep.CollectionItemEntity == collectionItemEntity
                                                                          select bep).FirstOrDefault();

                if (collectionItemNParticipant != null)
                {
                    unitOfWork.CollectionItemNParticipantRepository.Delete(collectionItemNParticipant);
                    unitOfWork.Save();
                }
            }
        }

        private List<string> ConnectPlaceToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int placeID, string relationship)
        {
            if (placeID <= 0)
            {
                return [];
            }
            Place? place = unitOfWork.PlaceRepository.GetByID(placeID);
            if (place is null)
            {
                return [];
            }

            List<CollectionItemRelationship> relationshipList = processRelationship.GetEntityListViaPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return [];
            }

            CollectionItemNPlace collectionItemEntityNPlace = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                PlaceID = placeID,
                RelationTypeId = relationshipList.First().CollectionItemRelationshipId
            };
            _ = unitOfWork.CollectionItemNPlaceRepository.Insert(collectionItemEntityNPlace);
            unitOfWork.Save();

            return [.. place.PlaceNToponymyList.Select(x => x.Toponymy.ToponymyName)];
        }
        private Dictionary<string, string> SyncPlaceConnections(CollectionItemEntity existingCollectionItemEntity, List<PlaceToCollectionItemCreateDTO> newConnections)
        {
            List<CollectionItemNPlace> currentConnections = existingCollectionItemEntity.CollectionItemNPlaceList;

            for (int i = currentConnections.Count - 1; i == 0; i--)
            {
                PlaceToCollectionItemCreateDTO? updated = newConnections.FirstOrDefault(x => x.Id == currentConnections[i].PlaceID);

                if (updated == null)
                {
                    DisconnectPlaceConnection(existingCollectionItemEntity, currentConnections[i].PlaceID);
                }
                else
                {
                    UpdateCollectionItemNPlace(existingCollectionItemEntity, updated);
                }
            }

            Dictionary<string, string> toponymyList = [];
            foreach (PlaceToCollectionItemCreateDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.Id);
                if (!exists)
                {
                    toponymyList.Add(nameof(Toponymy.ToponymyName), string.Join(", ", ConnectPlaceToCollectionItemEntity(existingCollectionItemEntity, newItem.Id, newItem.Relationship)));
                }
            }

            return toponymyList;
        }
        private void UpdateCollectionItemNPlace(CollectionItemEntity existingCollectionItemEntity, PlaceToCollectionItemCreateDTO updated)
        {
            List<CollectionItemRelationship> relationshipList = processRelationship.GetEntityListViaPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [updated.Relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return;
            }

            CIRelationshipDisplayDTO? relationship = processRelationship.GetWithTranslationsListViaPredicates(new CIRelationshipSearchParameterModel { CollectionItemRelationshipName = [updated.Relationship] }).FirstOrDefault();
            CollectionItemNPlace? collectionItemNPlace = (from bec in unitOfWork.CollectionItemNPlaceRepository.Get(includeProperties: nameof(Place))
                                                          where bec.PlaceID == updated.Id && bec.CollectionItemEntity == existingCollectionItemEntity
                                                          select bec).FirstOrDefault();
            if (collectionItemNPlace != null && relationship != null)
            {
                if (collectionItemNPlace.RelationTypeId != relationship.Id)
                {
                    collectionItemNPlace.RelationTypeId = relationship.Id;
                    unitOfWork.Save();
                }
            }
        }
        private void DisconnectPlaceConnection(CollectionItemEntity collectionItemEntity, int placeID)
        {
            if (collectionItemEntity.CollectionItemEntityID > 0 && placeID > 0)
            {
                CollectionItemNPlace? collectionItemEntityNPlace = (from bec in unitOfWork.CollectionItemNPlaceRepository.Get(includeProperties: "Place")
                                                                    where bec.PlaceID == placeID && bec.CollectionItemEntity == collectionItemEntity
                                                                    select bec).FirstOrDefault();

                if (collectionItemEntityNPlace != null)
                {
                    unitOfWork.CollectionItemNPlaceRepository.Delete(collectionItemEntityNPlace);
                    unitOfWork.Save();
                }
            }
        }

        private (List<(PictureToCollectionItemEditDTO CollectionItemPicture, int PictureId, string Process)>, int Statuscode, string Statusmessage)
        SyncCollectionItemPictureConnections(
        CollectionItemEntity existingCollectionItemEntity,
        List<PictureToCollectionItemEditDTO> newConnections,
        string? deletedPictureIds) // Neu: Parameter für gelöschte IDs
        {
            List<CollectionItemPicture> currentConnections = existingCollectionItemEntity.CollectionItemPictureList;
            List<(PictureToCollectionItemEditDTO CollectionItemPicture, int PictureId, string Process)> pictureResults = [];

            // Verarbeite explizit gelöschte IDs
            List<int> deletedIds = [];
            if (!string.IsNullOrEmpty(deletedPictureIds))
            {
                deletedIds = [.. deletedPictureIds.Split(',').Select(int.Parse)];

                foreach (var deletedId in deletedIds)
                {
                    var pictureToDelete = currentConnections.FirstOrDefault(x => x.CollectionItemPictureID == deletedId);
                    if (pictureToDelete != null)
                    {
                        (int statusCode, string statusMessage) = processCollectionItemPicture.Delete(pictureToDelete);
                        if (statusCode != 200)
                        {
                            return ([], statusCode, statusMessage);
                        }
                        PictureToCollectionItemEditDTO newEditDto = new()
                        {
                            Id = pictureToDelete.CollectionItemPictureID,
                            Perspective = pictureToDelete.Perspective
                        };
                        pictureResults.Add((newEditDto, pictureToDelete.CollectionItemPictureID, "delete"));
                    }
                }
            }

            foreach (PictureToCollectionItemEditDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.CollectionItemPictureID == newItem.Id);
                if (!exists)
                {
                    CollectionItemPicture newCollectionItemPicture = new()
                    {
                        CollectionItemEntityID = existingCollectionItemEntity.CollectionItemEntityID,
                        Perspective = newItem.Perspective,
                        IFormFile = newItem.IFormFile
                    };
                    (int statusCode, string statusMessage, int pictureId) = processCollectionItemPicture.Insert(newCollectionItemPicture);
                    if (statusCode != 200)
                    {
                        return ([], statusCode, statusMessage);
                    }
                    pictureResults.Add((newItem, pictureId, "insert"));
                }
            }

            return (pictureResults, 200, "Success_CollectionItemPicture_Synchronized");
        }

        private List<string> SyncConceptValueConnections(CollectionItemEntity existingCollectionItemEntity, List<ConceptValue> newConnections)
        {
            List<ConceptValue> currentConnections = existingCollectionItemEntity.ConceptValueList;
            List<string> translationList = [];

            for (int i = 0; i < currentConnections.Count; i++)
            {
                ConceptValue? updated = newConnections.FirstOrDefault(x => x.ConceptValueID == currentConnections[i].ConceptValueID);

                if (updated == null)
                {
                    processConceptValue.Delete(currentConnections[i].ConceptValueID);
                }
                else if (updated != null)
                {
                    translationList.AddRange(processConceptValue.Update(updated));
                }
            }

            foreach (ConceptValue newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ConceptID == newItem.ConceptID);
                if (!exists)
                {
                    translationList.AddRange(processConceptValue.Insert(newItem, existingCollectionItemEntity.CollectionItemEntityID));
                }
            }
            return translationList;
        }
    }
}
