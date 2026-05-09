using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEntity
    {
        List<CollectionItemDisplayDTO> GetWithPredicates(CollectionItemSearchParameterModel model);
        List<CollectionItemDisplayDTO> GetWithVector(CollectionItemSearchParameterModel model);
        List<CollectionItemDisplayDTO> GetTraditionalTextSearch(CollectionItemSearchParameterModel model);
        (int statusCode, string statusMessage) Insert(CollectionItemCreateDTO createDto, UsingIdentityUser user);
        (int statusCode, string statusMessage) Update(CollectionItemEditDTO editDTO);
        (int statusCode, string statusMessage) Delete(int id);
    }

    public class CollectionItemEntityProcessor(IUnitOfWork unitOfWork,
        IProcessCollectionItemPicture processCollectionItemPicture,
        IProcessPicturePhysically processPicturePhysically,
        IProcessConceptValue processConceptValue,
        IProcessConcept processConcept,
        IProcessCollectionItemEmbedding processCollectionItemEmbedding,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents,
        IProcessCIRelationship processRelationship) : IProcessCollectionItemEntity
    {
        public (int statusCode, string statusMessage) Insert(CollectionItemCreateDTO createDTO, UsingIdentityUser user)
        {
            List<(PictureToCollectionItemCreateDTO, int)> collectionItemPictureList = [];

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionItemEntity newCollectionItemEntity = new()
                {
                    CollectionAreaID = createDTO.CollectionAreaID,
                    UniqueName = createDTO.UniqueName,
                    Fake = createDTO.Fake,
                    Comment = createDTO.Comment,
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

                List<string> translationList = []; //For Vector Search
                if (!string.IsNullOrEmpty(createDTO.Comment))
                {
                    translationList.AddRange(processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Comment,
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Comment),
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    }));
                }
                if (!string.IsNullOrEmpty(createDTO.Inscription))
                {
                    translationList.AddRange(processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Inscription,
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Inscription),
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    }));
                }
                if (!string.IsNullOrEmpty(createDTO.UniqueName))
                {
                    translationList.AddRange(processTranslations.Insert(
                        new TranslationDTO
                        {
                            TextToTranslate = createDTO.UniqueName,
                            EntityType = nameof(CollectionItemEntity),
                            EntityId = newCollectionItemEntity.CollectionItemEntityID,
                            FieldName = nameof(CollectionItemEntity.UniqueName),
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        }));
                }

                foreach (ParticipantToCollectionItemCreateDTO createDto in createDTO.ParticipantctionItemList)
                {
                    translationList.AddRange(ConnectParticipantToCollectionItemEntity(newCollectionItemEntity, createDto.Id, createDto.Relationship));
                }
                foreach (PlaceToCollectionItemCreateDTO place in createDTO.PlaceToCollectionItemList)
                {
                    translationList.AddRange(ConnectPlaceToCollectionItemEntity(newCollectionItemEntity, place.Id, place.Relationship));
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
                    (int code, string returnMessage, List<string> coneptValueTranslationList) = processConceptValue.Insert(newConceptValue, newCollectionItemEntity.CollectionItemEntityID);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (code, returnMessage);
                    }
                    translationList.AddRange(coneptValueTranslationList);
                }

                (int statuscode, string statusmessage) = processCollectionItemEmbedding.Insert(newCollectionItemEntity, translationList);
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
                        PerspectiveInt = collectionItemPicture.Perspective
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
                return (500, "Error_Error_Ocurred");
            }
        }

        public (int statusCode, string statusMessage) Update(CollectionItemEditDTO editDto)
        {
            CollectionItemSearchParameterModel collectionItemSearchParameterModel = new();
            collectionItemSearchParameterModel.CollectionItemEntityID.Add(editDto.Id);
            CollectionItemEntity? existingEntity = GetWithPredicates(collectionItemSearchParameterModel).FirstOrDefault()?.CollectionItemEntity;
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

                List<string> translationList = [];
                int statuscode = 0;
                string statusmessage = "";

                translationList.AddRange(ChangeEntity(editDto, existingEntity));
                translationList.AddRange(SyncParticipantConnections(existingEntity, editDto.ConnectedParticipantList));
                translationList.AddRange(SyncPlaceConnections(existingEntity, editDto.ConnectedPlaceList));
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
                (statuscode, statusmessage, translationList) = SyncConceptValueConnections(existingEntity, conceptValueList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }
                (statuscode, statusmessage) = processCollectionItemEmbedding.Update(existingEntity, translationList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }

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
                        Perspective = collectionItemPicture.PerspectiveInt
                    };
                    //(statuscode, statusmessage) = processPicturePhysically.SaveCollectionItemPic(pictureToCollectionItemCreateDTO, pictureId, false, existingEntity.UsingIdentityUser.DisplayName); 
                    (statuscode, statusmessage) = processPicturePhysically.SaveCollectionItemPic(pictureToCollectionItemCreateDTO, pictureId, "Testuser");
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
                return (500, "Error_Error_Ocurred");
            }

            List<string> ChangeEntity(CollectionItemEditDTO editDto, CollectionItemEntity existingEntity)
            {
                bool hasChanges = false;
                List<string> translationList = [];

                var existingTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(CollectionItemEntity)],
                    EntityId = [existingEntity.CollectionItemEntityID]
                });

                string? translatedComment = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.Comment))?.TranslatedText;
                if (!string.IsNullOrEmpty(editDto.Comment))
                {
                    if (translatedComment != null)
                    {
                        if (translatedComment != editDto.Comment)
                        {
                            translationList.AddRange(processTranslations.Update(
                                new TranslationDTO
                                {
                                    TextToTranslate = editDto.Comment,
                                    EntityType = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    FieldName = nameof(CollectionItemEntity.Comment),
                                    Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                }));
                        }
                    }
                    else
                    {
                        translationList.AddRange(processTranslations.Insert(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.Comment,
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.Comment),
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            }));
                    }
                }
                else if (translatedComment != null)
                {
                    processTranslations.Delete(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(CollectionItemEntity)],
                        FieldName = [nameof(CollectionItemEntity.Comment)],
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

                string? translatedUniqueName = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.UniqueName))?.TranslatedText;
                if (!string.IsNullOrEmpty(editDto.UniqueName))
                {
                    if (translatedUniqueName != null)
                    {
                        if (translatedUniqueName != editDto.UniqueName)
                        {
                            translationList.AddRange(processTranslations.Update(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.UniqueName,
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.UniqueName),
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            }));
                        }
                    }
                    else
                    {
                        translationList.AddRange(processTranslations.Insert(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.UniqueName,
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.UniqueName),
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            }));
                    }
                }
                else if (translatedUniqueName != null)
                {
                    processTranslations.Delete(
                        new EntityTranslationSearchParameter
                        {
                            EntityType = [nameof(CollectionItemEntity)],
                            FieldName = [nameof(CollectionItemEntity.UniqueName)],
                            EntityId = [existingEntity.CollectionItemEntityID]
                        });
                }

                if (existingEntity.Inscription != editDto.Inscription)
                {
                    existingEntity.Inscription = editDto.Inscription;
                    hasChanges = true;

                    if (!string.IsNullOrEmpty(editDto.Inscription))
                    {
                        string? translatedInscription = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.InscriptionTranslated))?.TranslatedText;
                        if (translatedInscription != null)
                        {
                            if (translatedInscription != editDto.InscriptionTranslated)
                            {
                                translationList.AddRange(processTranslations.Update(
                                    new TranslationDTO
                                    {
                                        TextToTranslate = editDto.Inscription,
                                        EntityType = nameof(CollectionItemEntity),
                                        EntityId = existingEntity.CollectionItemEntityID,
                                        FieldName = nameof(CollectionItemEntity.Inscription),
                                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                    }));
                            }
                        }
                        else
                        {
                            translationList.AddRange(processTranslations.Insert(
                                new TranslationDTO
                                {
                                    TextToTranslate = editDto.Inscription,
                                    EntityType = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    FieldName = nameof(CollectionItemEntity.Inscription),
                                    Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                }));
                        }
                    }
                    else
                    {
                        processTranslations.Delete(new EntityTranslationSearchParameter
                        {
                            EntityType = [nameof(CollectionItemEntity)],
                            FieldName = [nameof(CollectionItemEntity.InscriptionTranslated)],
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

        public (int statusCode, string statusMessage) Delete(int id)
        {
            CollectionItemDisplayDTO? existingOperationParameterModel = GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [id] }).FirstOrDefault();
            if (existingOperationParameterModel == null)
            {
                return (400, "Error_CollectionItemEntity_NotFound");
            }
            List<int> collectionItemPictureList = [];

            try
            {
                using TransactionScope scope = new();

                for (int i = existingOperationParameterModel.CollectionItemNParticipantList.Count - 1; i == 0; i--)
                {
                    DisconnectParticipantConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNParticipantList[i].ParticipantID);
                }
                for (int i = existingOperationParameterModel.CollectionItemNPlaceList.Count - 1; i == 0; i--)
                {
                    DisconnectPlaceConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNPlaceList[i].PlaceID);
                }
                for (int i = existingOperationParameterModel.CollectionItemPictureList.Count - 1; i == 0; i--)
                {
                    collectionItemPictureList.Add(existingOperationParameterModel.CollectionItemPictureList[i].CollectionItemPictureID);
                    (int statuscode, string statusmessage) = processCollectionItemPicture.Delete(existingOperationParameterModel.CollectionItemPictureList[i]);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                for (int i = existingOperationParameterModel.ConceptValueList.Count - 1; i == 0; i--)
                {
                    (int statuscode, string statusmessage) = processConceptValue.Delete(existingOperationParameterModel.ConceptValueList[i].ConceptValueID);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }

                unitOfWork.CollectionItemEntityRepository.Delete(existingOperationParameterModel.CollectionItemEntity);
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
                return (500, "Error_Error_Ocurred");
            }
        }

        public List<CollectionItemDisplayDTO> GetWithPredicates(CollectionItemSearchParameterModel model)
        {
            IEnumerable<CollectionItemEntity> collectionItemIEnumberable = unitOfWork.CollectionItemEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemEntity>(model),
                includeProperties: GetIncludeProperties());

            List<CollectionItemDisplayDTO> collectionItemList = [..from b in collectionItemIEnumberable
                      select SetMembersofEntity(b)];

            collectionItemList = SetTranslations(collectionItemList);

            return [.. collectionItemList.OrderBy(x => x.CollectionItemEntity.PersonalIdentificationNumber).ThenBy(x => x.CollectionItemEntity.CollectionItemEntityID)];
        }
        public List<CollectionItemDisplayDTO> GetTraditionalTextSearch(CollectionItemSearchParameterModel model)
        {
            trackEvents.TrackInfo("GetTraditionalTextSearch_Started", new Dictionary<string, object>
            {
                { "SemanticSearchQuery", model.SemanticSearchQuery ?? "null" }
            });

            if (string.IsNullOrEmpty(model.SemanticSearchQuery))
                return GetWithPredicates(model);

            var searchTerms = model.SemanticSearchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var results = unitOfWork.CollectionItemEntityRepository.Get(
                includeProperties: GetIncludeProperties(),
                filter: item =>
                    searchTerms.Any(term =>
                        (item.SerialNumber != null && item.SerialNumber.Contains(term)) ||
                        (item.PersonalIdentificationNumber != null && item.PersonalIdentificationNumber.Contains(term)) ||
                        (item.CollectionItemEntityID == translationStore.GetId<CollectionItemEntity>(term)) ||
                        (item.StatePreservationID == translationStore.GetId<StatePreservation>(term)) ||
                        (item.Inscription != null && item.Inscription.Contains(term)) ||
                        item.CollectionItemNPlaceList.Any(p =>
                            p.Place != null && p.Place.PlaceID == translationStore.GetId<Place>(term)) ||
                        item.CollectionItemNParticipantList.Any(p =>
                            p.Participant != null && p.Participant.ParticipantID == translationStore.GetId<Participant>(term)) ||
                        (item.Era != null && item.Era.EraID == translationStore.GetId<Era>(term))
                    )
            );

            return [.. from b in results
               select SetMembersofEntity(b)];
        }
        private static string GetIncludeProperties()
        {
            return nameof(CollectionItemEntity.CollectionItemPictureList) + "," +
                   //Für test ausgenommen
                   nameof(CollectionItemEntity.UsingIdentityUser) + "," +
                   nameof(CollectionItemEntity.CollectionArea) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPlaceList) + "." + nameof(CollectionItemNPlace.Place) +
                       "." + nameof(Place.PlaceNToponymyList) + "." + nameof(PlaceNToponymy.Toponymy) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPlaceList) + "." + nameof(CollectionItemNPlace.RelationType) + "," +
                   nameof(CollectionItemEntity.CollectionItemNParticipantList) + "." + nameof(CollectionItemNParticipant.Participant) + "," +
                   nameof(CollectionItemEntity.CollectionItemNParticipantList) + "." + nameof(CollectionItemNParticipant.RelationType) + "," +
                   nameof(CollectionItemEntity.ConceptValueList);
        }
        private CollectionItemDisplayDTO SetMembersofEntity(CollectionItemEntity b)
        {
            var conceptList = processConcept.Get(new ConceptualRelationshipSearchParameterModel { }).Select(x => x.ConceptViewModel).ToList();
            List<ConceptValue> cvL = b.ConceptValueList;
            foreach (var cv in cvL)
            {
                cv.ConceptName = conceptList.FirstOrDefault(x => x.Id == cv.ConceptID)?.Name;
            }
            return new CollectionItemDisplayDTO
            {
                CollectionItemEntity = b,
                CollectionItemPictureList = b.CollectionItemPictureList,
                StatePreservationList = [.. unitOfWork.StateRepository.Get()],
                ConceptValueList = cvL,
                CollectionItemNParticipantList = b.CollectionItemNParticipantList,
                CollectionItemNPlaceList = b.CollectionItemNPlaceList,
                Era = b.Era ?? new() { EraName = string.Empty },
                CvmList = [.. conceptList.Where(x => x.CollectionAreaID == b.CollectionAreaID || x.CollectionAreaID == 0)]
            };
        }
        private List<CollectionItemDisplayDTO> SetTranslations(List<CollectionItemDisplayDTO> operationParameterList)
        {
            List<CollectionItemDisplayDTO> ciopList = operationParameterList;
            foreach (var ciop in operationParameterList)
            {
                ciop.CollectionItemEntity.UniqueName = translationStore.GetTranslation(
                    nameof(CollectionItemEntity),
                    ciop.CollectionItemEntity.CollectionItemEntityID,
                    nameof(CollectionItemEntity.UniqueName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
                ciop.CollectionItemEntity.InscriptionTranslated = translationStore.GetTranslation(
                    nameof(CollectionItemEntity),
                    ciop.CollectionItemEntity.CollectionItemEntityID,
                    nameof(CollectionItemEntity.InscriptionTranslated),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
                ciop.CollectionItemEntity.Comment = translationStore.GetTranslation(
                    nameof(CollectionItemEntity),
                    ciop.CollectionItemEntity.CollectionItemEntityID,
                    nameof(CollectionItemEntity.Comment),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
                if (ciop.CollectionItemEntity.EraID > 0)
                {
                    ciop.Era.EraName = translationStore.GetTranslation(
                        nameof(Era),
                        (int)ciop.CollectionItemEntity.EraID,
                        nameof(Era.EraName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                        ?? string.Empty;
                }
                if (ciop.CollectionItemEntity.StatePreservationID > 0)
                {
                    ciop.CollectionItemEntity.StatePreservation?.StatePreservationName = translationStore.GetTranslation(
                        nameof(StatePreservation),
                        (int)ciop.CollectionItemEntity.StatePreservationID,
                        nameof(StatePreservation.StatePreservationName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)) ?? string.Empty;
                }
                foreach (var cav in ciop.ConceptValueList)
                {
                    cav.ValueString = translationStore.GetTranslation(
                        nameof(ConceptValue),
                        cav.ConceptValueID,
                        nameof(ConceptValue.ValueString),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name));
                }
                foreach (var placeConnection in ciop.CollectionItemNPlaceList)
                {
                    placeConnection.RelationType.CollectionItemRelationshipName = translationStore.GetTranslation(
                        nameof(CollectionItemRelationship),
                        placeConnection.RelationTypeId,
                        nameof(CollectionItemRelationship.CollectionItemRelationshipName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                        ?? string.Empty;
                }
                foreach (var participantConnection in ciop.CollectionItemNParticipantList)
                {
                    participantConnection.RelationType.CollectionItemRelationshipName = translationStore.GetTranslation(
                        nameof(CollectionItemRelationship),
                        participantConnection.RelationTypeId,
                        nameof(CollectionItemRelationship.CollectionItemRelationshipName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                        ?? string.Empty;
                }
            }

            return ciopList;
        }

        public List<CollectionItemDisplayDTO> GetWithVector(CollectionItemSearchParameterModel model)
        {
            if (string.IsNullOrEmpty(model.SemanticSearchQuery))
                return GetWithPredicates(model);

            var vectorResults = processCollectionItemEmbedding.Search(model.SemanticSearchQuery);

            //// Filtern nach Minimum Similarity Score
            //if (createDTO.MinimumSimilarityScore.HasValue)
            //{
            //    vectorResults = [.. vectorResults.Where(x => x.SimilarityScore >= createDTO.MinimumSimilarityScore.Value)];
            //}
            if (vectorResults.Count == 0)
            {
                return [];
            }

            model.CollectionItemEntityID = [.. vectorResults.Select(x => x.CollectionItemEntityID)];
            return GetWithPredicates(model);
        }

        private List<string> ConnectParticipantToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int participantID, string relationship)
        {
            if (participantID <= 0)
            {
                return [];
            }
            Participant? participant = unitOfWork.ParticipantRepository.GetByID(participantID);
            if (participant is null)
            {
                return [];
            }

            List<CollectionItemRelationship> relationshipList = processRelationship.GetListWithPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return [];
            }

            CollectionItemNParticipant collectionItemEntityNParticipant = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                ParticipantID = participantID,
                RelationTypeId = relationshipList.First().CollectionItemRelationshipId
            };
            _ = unitOfWork.CollectionItemNParticipantRepository.Insert(collectionItemEntityNParticipant);
            unitOfWork.Save();

            return translationStore.GetById<Participant>(participantID);
        }
        private List<string> SyncParticipantConnections(CollectionItemEntity existingCollectionItemEntity, List<ParticipantToCollectionItemCreateDTO> newConnections)
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

            List<string> translationList = [];
            foreach (ParticipantToCollectionItemCreateDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ParticipantID == newItem.Id);
                if (!exists)
                {
                    translationList.AddRange(ConnectParticipantToCollectionItemEntity(existingCollectionItemEntity, newItem.Id, newItem.Relationship));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNParticipant(CollectionItemEntity existingCollectionItemEntity, ParticipantToCollectionItemCreateDTO updated)
        {
            List<CollectionItemRelationship> relationshipList = processRelationship.GetListWithPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [updated.Relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return;
            }

            CollectionItemNParticipant? collectionItemNParticipant = (from bep in unitOfWork.CollectionItemNParticipantRepository.Get(includeProperties: nameof(Participant))
                                                          where bep.ParticipantID == updated.Id && bep.CollectionItemEntity == existingCollectionItemEntity
                                                          select bep).FirstOrDefault();

            if (collectionItemNParticipant != null)
            {
                if (collectionItemNParticipant.RelationType.CollectionItemRelationshipName != updated.Relationship)
                {
                    collectionItemNParticipant.RelationTypeId = relationshipList.First().CollectionItemRelationshipId;
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

            List<CollectionItemRelationship> relationshipList = processRelationship.GetListWithPredicates(new CIRelationshipSearchParameterModel
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

            return translationStore.GetById<Place>(placeID);
        }
        private List<string> SyncPlaceConnections(CollectionItemEntity existingCollectionItemEntity, List<PlaceToCollectionItemCreateDTO> newConnections)
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

            List<string> translationList = [];
            foreach (PlaceToCollectionItemCreateDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.Id);
                if (!exists)
                {
                    translationList.AddRange(ConnectPlaceToCollectionItemEntity(existingCollectionItemEntity, newItem.Id, newItem.Relationship));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNPlace(CollectionItemEntity existingCollectionItemEntity, PlaceToCollectionItemCreateDTO updated)
        {
            List<CollectionItemRelationship> relationshipList = processRelationship.GetListWithPredicates(new CIRelationshipSearchParameterModel
            {
                CollectionItemRelationshipName = [updated.Relationship]
            });
            if (relationshipList.Count == 0 || relationshipList.Count > 1)
            {
                return;
            }

            CollectionItemNPlace? collectionItemNPlace = (from bec in unitOfWork.CollectionItemNPlaceRepository.Get(includeProperties: "Place")
                                                          where bec.PlaceID == updated.Id && bec.CollectionItemEntity == existingCollectionItemEntity
                                                          select bec).FirstOrDefault();
            if (collectionItemNPlace != null)
            {
                if (collectionItemNPlace.RelationType.CollectionItemRelationshipName != updated.Relationship)
                {
                    collectionItemNPlace.RelationTypeId = relationshipList.First().CollectionItemRelationshipId;
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

        private (int Statuscode, string Statusmessage, List<string> TranslationList) SyncConceptValueConnections(CollectionItemEntity existingCollectionItemEntity, List<ConceptValue> newConnections)
        {
            List<ConceptValue> currentConnections = existingCollectionItemEntity.ConceptValueList;
            int statusCode;
            string statusMessage;
            List<string> translationList = [];

            for (int i = 0; i < currentConnections.Count; i++)
            {
                ConceptValue? updated = newConnections.FirstOrDefault(x => x.ConceptValueID == currentConnections[i].ConceptValueID);

                if (updated == null)
                {
                    (statusCode, statusMessage) = processConceptValue.Delete(currentConnections[i].ConceptValueID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
                else if (updated != null)
                {
                    (statusCode, statusMessage, translationList) = processConceptValue.Update(updated);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
            }

            foreach (ConceptValue newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ConceptID == newItem.ConceptID);
                if (!exists)
                {
                    (statusCode, statusMessage, translationList) = processConceptValue.Insert(newItem, existingCollectionItemEntity.CollectionItemEntityID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
            }
            return (200, "Success_ConceptValue_Synchronized", translationList);
        }
    }
}
