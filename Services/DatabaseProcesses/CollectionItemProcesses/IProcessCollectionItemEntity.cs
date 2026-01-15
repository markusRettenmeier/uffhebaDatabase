using Microsoft.CodeAnalysis;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.Translation;
using System.Linq;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEntity
    {
        List<CollectionItemOperationParameterModel> GetWithPredicates(CollectionItemSearchParameterModel model); 
        List<CollectionItemOperationParameterModel> GetWithVector(CollectionItemSearchParameterModel model);
        List<CollectionItemOperationParameterModel> GetTraditionalTextSearch(CollectionItemSearchParameterModel model);
        (int statusCode, string statusMessage) Insert(CollectionItemOperationParameterModel model);
        (int statusCode, string statusMessage) Update(CollectionItemOperationParameterModel model);
        (int statusCode, string statusMessage) Delete(CollectionItemOperationParameterModel model);
    }

    public class CollectionItemEntityProcessor(IUnitOfWork unitOfWork,
        IProcessCollectionItemPicture processCollectionItemPicture,
        IProcessPicturePhysically processPicturePhysically,
        IProcessConceptValue processConceptValue,
        IProcessCollectionItemEmbedding processCollectionItemEmbedding,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEvents trackEvents) : IProcessCollectionItemEntity
    {
        public CollectionItemSearchParameterModel ParametersOperationToSearch(CollectionItemOperationParameterModel operationParameterModel)
        {
            CollectionItemSearchParameterModel searchParameterModel = new();
            searchParameterModel.CollectionItemEntityID.Add(operationParameterModel.CollectionItemEntity.CollectionItemEntityID);
            if (!string.IsNullOrEmpty(operationParameterModel.CollectionItemEntity.UniqueName))
            {
                searchParameterModel.UniqueName.Add(operationParameterModel.CollectionItemEntity.UniqueName);
            }

            return searchParameterModel;
        }

        public (int statusCode, string statusMessage) Insert(CollectionItemOperationParameterModel model)
        {
            if (model.CollectionItemEntity.CollectionAreaID <= 0)
            {
                trackEvents.TrackWarning ("InsertCollectionItemEntity_Failed_MissingCollectionAreaID");
                return (400, "Error_CollectionArea_IdMissing");
            }
            if (string.IsNullOrEmpty(model.CollectionItemEntity.UsingIdentityUsersID))
            {
                trackEvents.TrackWarning ("InsertCollectionItemEntity_Failed_MissingUserID");
                return (400, "Error_UserID_Missing");
            }

            List<(CollectionItemPicture, int)> pictureList = [];

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionItemEntity newCollectionItemEntity = unitOfWork.CollectionItemEntityRepository.Insert(model.CollectionItemEntity);
                unitOfWork.Save();

                List<string> translationList = []; //For Vector Search
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.Comment))
                {
                    translationList.AddRange(processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Comment),
                        TranslatedText = newCollectionItemEntity.Comment ?? string.Empty,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    model.CollectionItemEntity.Comment));
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.Inscription))
                {
                    translationList.AddRange(processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Inscription),
                        TranslatedText = newCollectionItemEntity.Inscription ?? string.Empty,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    model.CollectionItemEntity.Inscription));
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.UniqueName))
                {
                    translationList.AddRange(processTranslations.Insert(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionItemEntity),
                            EntityId = newCollectionItemEntity.CollectionItemEntityID,
                            FieldName = nameof(CollectionItemEntity.UniqueName),
                            TranslatedText = newCollectionItemEntity.UniqueName ?? string.Empty,
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                        },
                        model.CollectionItemEntity.UniqueName));
                }

                foreach (CollectionItemNParty entityNParty in model.CollectionItemNPartyList)
                {
                    translationList.AddRange(ConnectPartyToCollectionItemEntity(newCollectionItemEntity, entityNParty.PartyID, entityNParty.Relationship));
                }
                foreach (CollectionItemNPlace collectionItemEntityNPlace in model.CollectionItemNPlaceList)
                {
                    translationList.AddRange(ConnectPlaceToCollectionItemEntity(newCollectionItemEntity, collectionItemEntityNPlace.PlaceID, collectionItemEntityNPlace.Relationship));
                }
                foreach (ConceptValue coneptValue in model.ConceptValueList)
                {
                    (int code, string returnMessage, translationList) = processConceptValue.Insert(coneptValue, newCollectionItemEntity.CollectionItemEntityID);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (code, returnMessage);
                    }
                }

                (int statuscode, string statusmessage) =  processCollectionItemEmbedding.Insert(newCollectionItemEntity, translationList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }

                foreach (CollectionItemPicture collectionItemPicture in model.CollectionItemPictureList)
                {
                    (int code, string returnMessage, int pictureId) = processCollectionItemPicture.Insert(collectionItemPicture, newCollectionItemEntity);
                    pictureList.Add((collectionItemPicture, pictureId));
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (code, returnMessage);
                    }
                }
                foreach (var picture in pictureList)
                {
                    (int code, string returnMessage) = processPicturePhysically.Save(picture.Item1, picture.Item2, false);
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
                    { "CollectionItemEntity", model.CollectionItemEntity },
                    { "ConceptValueConnections", model.ConceptValueList },
                    { "PictureList", model.CollectionItemPictureList },
                    { "PartyConnections", model.CollectionItemNPartyList },
                    { "PlaceConnections", model.CollectionItemNPlaceList },
                    { "Era", model.Era },
                    { "CollectionSet", model.CollectionSet }
                });
                return (500, "Error_Error_Ocurred");
            }
        }

        public (int statusCode, string statusMessage) Update(CollectionItemOperationParameterModel operationModel)
        {
            CollectionItemSearchParameterModel collectionItemSearchParameterModel = new();
            collectionItemSearchParameterModel.CollectionItemEntityID.Add(operationModel.CollectionItemEntity.CollectionItemEntityID);
            CollectionItemEntity? existingEntity = GetWithPredicates(collectionItemSearchParameterModel).FirstOrDefault()?.CollectionItemEntity;
            if (existingEntity == null)
            {
                trackEvents.TrackWarning ("UpdateCollectionItemEntity_Failed_NotFound", new Dictionary<string, object>
                {
                    { "CollectionItemEntityID", operationModel.CollectionItemEntity.CollectionItemEntityID.ToString() }
                });
                return (400, "Error_CollectionItemEntity_NotFound");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                List<string> translationList = [];
                int statuscode = 0;
                string statusmessage = "";

                translationList.AddRange(ChangeEntity(operationModel, existingEntity));
                translationList.AddRange(SyncPartyConnections(existingEntity, operationModel.CollectionItemNPartyList));
                translationList.AddRange(SyncPlaceConnections(existingEntity, operationModel.CollectionItemNPlaceList));
                (statuscode, statusmessage, translationList) = SyncConceptValueConnections(existingEntity, operationModel.ConceptValueList);
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

                (List<(CollectionItemPicture collectionItemPicture, int pictureId, string process)> pictureList, statuscode, statusmessage) = SyncPictureConnections(existingEntity, operationModel.CollectionItemPictureList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statuscode, statusmessage);
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "insert"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Save(collectionItemPicture, pictureId, false); 
                    if (statuscode != 200)
                    {
                        foreach (var pic in pictureList)
                        {
                            processPicturePhysically.Delete(pic.pictureId);
                        }
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "update"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Save(collectionItemPicture, pictureId, true); 
                    if (statuscode != 200)
                    {
                        foreach (var pic in pictureList)
                        {
                            processPicturePhysically.Delete(pic.pictureId);
                        }
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "delete"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Delete(pictureId); 
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
                        { "CollectionItemEntity", operationModel.CollectionItemEntity },
                        { "ConceptValueConnections", operationModel.ConceptValueList },
                        { "PictureList", operationModel.CollectionItemPictureList },
                        { "PartyConnections", operationModel.CollectionItemNPartyList },
                        { "PlaceConnections", operationModel.CollectionItemNPlaceList },
                        { "Era", operationModel.Era },
                        { "CollectionSet", operationModel.CollectionSet }
                    });
                return (500, "Error_Error_Ocurred");
            }

            List<string> ChangeEntity(CollectionItemOperationParameterModel operationModel, CollectionItemEntity existingEntity)
            {
                bool hasChanges = false;
                List<string> translationList = [];

                var existingTranslations = processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(CollectionItemEntity)],
                    EntityId = [existingEntity.CollectionItemEntityID]
                });

                string? translatedComment = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.Comment))?.TranslatedText;
                if (!string.IsNullOrEmpty(operationModel.CollectionItemEntity.Comment))
                {
                    if (translatedComment != null)
                    {
                        if (translatedComment != operationModel.CollectionItemEntity.Comment)
                        {
                            translationList.AddRange(processTranslations.Update(
                                new EntityTranslation
                                {
                                    EntityType = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    FieldName = nameof(CollectionItemEntity.Comment),
                                    TranslatedText = existingEntity.Comment ?? string.Empty,
                                    Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                },
                                operationModel.CollectionItemEntity.Comment));
                        }
                    }
                    else
                    {
                        translationList.AddRange(processTranslations.Insert(
                            new EntityTranslation
                            {
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.Comment),
                                TranslatedText = existingEntity.Comment ?? string.Empty,
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            },
                            operationModel.CollectionItemEntity.Comment));
                    }
                } else if(translatedComment != null)
                {
                    processTranslations.Delete(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(CollectionItemEntity)],
                        FieldName = [nameof(CollectionItemEntity.Comment)],
                        EntityId = [existingEntity.CollectionItemEntityID]
                    });
                }
                if (existingEntity.Width != operationModel.CollectionItemEntity.Width)
                {
                    existingEntity.Width = operationModel.CollectionItemEntity.Width;
                    hasChanges = true;
                }
                if (existingEntity.Height != operationModel.CollectionItemEntity.Height)
                {
                    existingEntity.Height = operationModel.CollectionItemEntity.Height;
                    hasChanges = true;
                }
                if (existingEntity.Length != operationModel.CollectionItemEntity.Length)
                {
                    existingEntity.Length = operationModel.CollectionItemEntity.Length;
                    hasChanges = true;
                }
                if (existingEntity.Diameter != operationModel.CollectionItemEntity.Diameter)
                {
                    existingEntity.Diameter = operationModel.CollectionItemEntity.Diameter;
                    hasChanges = true;
                }
                if (existingEntity.Weight != operationModel.CollectionItemEntity.Weight)
                {
                    existingEntity.Weight = operationModel.CollectionItemEntity.Weight;
                    hasChanges = true;
                }
                if (existingEntity.StatePreservationID != operationModel.CollectionItemEntity.StatePreservationID)
                {
                    existingEntity.StatePreservationID = operationModel.CollectionItemEntity.StatePreservationID;
                    hasChanges = true;
                }
                if (existingEntity.Fake != operationModel.CollectionItemEntity.Fake)
                {
                    existingEntity.Fake = operationModel.CollectionItemEntity.Fake;
                    hasChanges = true;
                }
                if (existingEntity.FilingLocation != operationModel.CollectionItemEntity.FilingLocation)
                {
                    existingEntity.FilingLocation = operationModel.CollectionItemEntity.FilingLocation;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryPrice != operationModel.CollectionItemEntity.DeliveryPrice)
                {
                    existingEntity.DeliveryPrice = operationModel.CollectionItemEntity.DeliveryPrice;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryDate != operationModel.CollectionItemEntity.DeliveryDate)
                {
                    existingEntity.DeliveryDate = operationModel.CollectionItemEntity.DeliveryDate;
                    hasChanges = true;
                }
                if (existingEntity.DeliveryAdress != operationModel.CollectionItemEntity.DeliveryAdress)
                {
                    existingEntity.DeliveryAdress = operationModel.CollectionItemEntity.DeliveryAdress;
                    hasChanges = true;
                }
                if (existingEntity.StartYear != operationModel.CollectionItemEntity.StartYear)
                {
                    existingEntity.StartYear = operationModel.CollectionItemEntity.StartYear;
                    hasChanges = true;
                }
                if (existingEntity.EndYear != operationModel.CollectionItemEntity.EndYear)
                {
                    existingEntity.EndYear = operationModel.CollectionItemEntity.EndYear;
                    hasChanges = true;
                }
                if (existingEntity.ExactYear != operationModel.CollectionItemEntity.ExactYear)
                {
                    existingEntity.ExactYear = operationModel.CollectionItemEntity.ExactYear;
                    hasChanges = true;
                }
                if (existingEntity.IsApproximate != operationModel.CollectionItemEntity.IsApproximate)
                {
                    existingEntity.IsApproximate = operationModel.CollectionItemEntity.IsApproximate;
                    hasChanges = true;
                }
                if (existingEntity.ConceptID != operationModel.CollectionItemEntity.ConceptID)
                {
                    existingEntity.ConceptID = operationModel.CollectionItemEntity.ConceptID;
                    hasChanges = true;
                }
                string? translatedUniqueName = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.UniqueName))?.TranslatedText;
                if (!string.IsNullOrEmpty(operationModel.CollectionItemEntity.UniqueName)) {
                    if (translatedUniqueName != null)
                    {
                        if (translatedUniqueName != operationModel.CollectionItemEntity.UniqueName)
                        {
                            translationList.AddRange(processTranslations.Update(
                            new EntityTranslation
                            {
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.UniqueName),
                                TranslatedText = existingEntity.UniqueName ?? string.Empty,
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            },
                            operationModel.CollectionItemEntity.UniqueName));
                        }
                    }
                    else
                    {
                        translationList.AddRange(processTranslations.Insert(
                            new EntityTranslation
                            {
                                EntityType = nameof(CollectionItemEntity),
                                EntityId = existingEntity.CollectionItemEntityID,
                                FieldName = nameof(CollectionItemEntity.UniqueName),
                                TranslatedText = existingEntity.UniqueName ?? string.Empty,
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                            },
                            operationModel.CollectionItemEntity.UniqueName));
                    }
                } else if(translatedUniqueName != null)
                {
                    processTranslations.Delete(
                        new EntityTranslationSearchParameter
                        {
                            EntityType = [nameof(CollectionItemEntity)],
                            FieldName = [nameof(CollectionItemEntity.UniqueName)],
                            EntityId = [existingEntity.CollectionItemEntityID]
                        });
                }
                if (existingEntity.Inscription != operationModel.CollectionItemEntity.Inscription)
                {
                    existingEntity.Inscription = operationModel.CollectionItemEntity.Inscription;
                    hasChanges = true;

                    if (!string.IsNullOrEmpty(operationModel.CollectionItemEntity.Inscription))
                    {
                        string? translatedInscription = existingTranslations.FirstOrDefault(x => x.FieldName == nameof(CollectionItemEntity.InscriptionTranslated))?.TranslatedText;
                        if (translatedInscription != null)
                        {
                            if (translatedInscription != operationModel.CollectionItemEntity.InscriptionTranslated)
                            {
                                translationList.AddRange(processTranslations.Update(
                                    new EntityTranslation
                                    {
                                        EntityType = nameof(CollectionItemEntity),
                                        EntityId = existingEntity.CollectionItemEntityID,
                                        FieldName = nameof(CollectionItemEntity.Inscription),
                                        TranslatedText = existingEntity.Inscription ?? string.Empty,
                                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                    },
                                    operationModel.CollectionItemEntity.Inscription));
                            }
                        }
                        else
                        {
                            translationList.AddRange(processTranslations.Insert(
                                new EntityTranslation
                                {
                                    EntityType = nameof(CollectionItemEntity),
                                    EntityId = existingEntity.CollectionItemEntityID,
                                    FieldName = nameof(CollectionItemEntity.Inscription),
                                    TranslatedText = existingEntity.Inscription ?? string.Empty,
                                    Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                                },
                                operationModel.CollectionItemEntity.Inscription));
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
                if (existingEntity.PersonalIdentificationNumber != operationModel.CollectionItemEntity.PersonalIdentificationNumber)
                {
                    existingEntity.PersonalIdentificationNumber = operationModel.CollectionItemEntity.PersonalIdentificationNumber;
                    hasChanges = true;
                }
                if (existingEntity.EraID != operationModel.CollectionItemEntity.EraID)
                {
                    existingEntity.EraID = operationModel.CollectionItemEntity.EraID;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    unitOfWork.Save();
                }
                return translationList;
            }
        }

        public (int statusCode, string statusMessage) Delete(CollectionItemOperationParameterModel model)
        {
            CollectionItemOperationParameterModel? existingOperationParameterModel = GetWithPredicates(ParametersOperationToSearch(model)).FirstOrDefault();
            if (existingOperationParameterModel == null)
            {
                return (400, "Error_CollectionItemEntity_NotFound");
            }
            List<int> picutureIdList = [];

            try
            {
                using TransactionScope scope = new();

                for (int i = existingOperationParameterModel.CollectionItemNPartyList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectPartyConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNPartyList[index].PartyID);
                }
                for (int i = existingOperationParameterModel.CollectionItemNPlaceList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectPlaceConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNPlaceList[index].PlaceID);
                }
                for (int i = existingOperationParameterModel.CollectionItemPictureList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    picutureIdList.Add(existingOperationParameterModel.CollectionItemPictureList[index].CollectionItemPictureID);
                    (int statuscode, string statusmessage) = processCollectionItemPicture.Delete(existingOperationParameterModel.CollectionItemPictureList[index]);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }
                for (int i = existingOperationParameterModel.ConceptValueList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    (int statuscode, string statusmessage) = processConceptValue.Delete(existingOperationParameterModel.ConceptValueList[index].ConceptValueID);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statuscode, statusmessage);
                    }
                }

                unitOfWork.CollectionItemEntityRepository.Delete(existingOperationParameterModel.CollectionItemEntity);
                unitOfWork.Save();

                foreach (int pictureID in picutureIdList)
                {
                    (int statuscode, string statusmessage) = processPicturePhysically.Delete(pictureID);
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
                    { "CollectionItemEntity", model.CollectionItemEntity },
                    { "ConceptValueConnections", model.ConceptValueList },
                    { "PictureList", model.CollectionItemPictureList },
                    { "PartyConnections", model.CollectionItemNPartyList },
                    { "PlaceConnections", model.CollectionItemNPlaceList },
                    { "Era", model.Era },
                    { "CollectionSet", model.CollectionSet }
                });
                return (500, "Error_Error_Ocurred");
            }
        }

        public List<CollectionItemOperationParameterModel> GetWithPredicates(CollectionItemSearchParameterModel model)
        {
            IEnumerable<CollectionItemEntity> collectionItemIEnumberable = unitOfWork.CollectionItemEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemEntity>(model),
                includeProperties: GetIncludeProperties());

            List<CollectionItemOperationParameterModel> collectionItemList = [..from b in collectionItemIEnumberable
                      select SetMembersofEntity(b)];

            collectionItemList = SetTranslations(collectionItemList);

            return [.. collectionItemList.OrderBy(x => x.CollectionItemEntity.PersonalIdentificationNumber).ThenBy(x => x.CollectionItemEntity.CollectionItemEntityID)];
        }
        public List<CollectionItemOperationParameterModel> GetTraditionalTextSearch(CollectionItemSearchParameterModel model)
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
                        (item.CollectionSetId == translationStore.GetId<CollectionSet>(term)) ||
                        (item.Inscription != null && item.Inscription.Contains(term)) ||
                        item.CollectionItemNPlaceList.Any(p =>
                            p.Place != null && p.Place.PlaceID == translationStore.GetId<Place>(term)) ||
                        item.CollectionItemNPartyList.Any(p =>
                            p.Party != null && p.Party.PartyID == translationStore.GetId<Party>(term)) ||
                        (item.Concept != null && item.Concept.Id == translationStore.GetId<Concept>(term)) ||
                        (item.Era != null && item.Era.EraID == translationStore.GetId<Era>(term))
                    )
            );

            return [.. from b in results
               select SetMembersofEntity(b)];
        }
        private static string GetIncludeProperties()
        {
            return nameof(CollectionItemEntity.CollectionItemPictureList) + "," +
                   nameof(CollectionItemEntity.UsingIdentityUser) + "," +
                   nameof(CollectionItemEntity.Concept) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPlaceList) + "." + nameof(CollectionItemNPlace.Place) +
                       "." + nameof(Place.PlaceNToponymyList) + "." + nameof(PlaceNToponymy.Toponymy) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPlaceList) + "." + nameof(CollectionItemNPlace.Place) +
                       "." + nameof(Place.Settlement) + "." + nameof(Settlement.SettlementNPostalcodeList) +
                       "." + nameof(SettlementNPostalcode.Postalcode) + "," +
                   nameof(CollectionItemEntity.CollectionItemNPartyList) + "." + nameof(CollectionItemNParty.Party) + "," +
                   nameof(CollectionItemEntity.Era) + "," +
                   nameof(CollectionItemEntity.StatePreservation) + "," +
                   nameof(CollectionItemEntity.CollectionSet) + "," +
                   nameof(CollectionItemEntity.CollectionArea) + "," +
                   nameof(CollectionItemEntity.ConceptValueList);
        }
        private CollectionItemOperationParameterModel SetMembersofEntity(CollectionItemEntity b)
        {
            return new CollectionItemOperationParameterModel
            {
                CollectionItemEntity = b,
                CollectionItemPictureList = b.CollectionItemPictureList,
                StatePreservationList = [.. unitOfWork.StateRepository.Get()],
                ConceptValueList = b.ConceptValueList,
                CollectionItemNPartyList = b.CollectionItemNPartyList,
                CollectionItemNPlaceList = b.CollectionItemNPlaceList,
                Era = b.Era ?? new() { EraName = string.Empty },
                ConceptList = [.. unitOfWork.ConceptRepository.Get(filter: x => x.CollectionAreaID == 0 || x.CollectionAreaID == b.CollectionAreaID)],
            };
        }
        private List<CollectionItemOperationParameterModel> SetTranslations (List<CollectionItemOperationParameterModel> operationParameterList)
        {
            List<CollectionItemOperationParameterModel> ciopList = operationParameterList;
            foreach (var ciop in operationParameterList)
            {
                if (!string.IsNullOrEmpty(ciop.CollectionItemEntity.UniqueName))
                {
                    ciop.CollectionItemEntity.UniqueName = translationStore.GetTranslation(
                        nameof(CollectionItemEntity),
                        ciop.CollectionItemEntity.CollectionItemEntityID,
                        nameof(CollectionItemEntity.UniqueName),
                        ciop.CollectionItemEntity.UniqueName) ?? ciop.CollectionItemEntity.UniqueName;
                }
                if (!string.IsNullOrEmpty(ciop.CollectionItemEntity.Inscription))
                {
                    ciop.CollectionItemEntity.InscriptionTranslated = translationStore.GetTranslation(
                        nameof(CollectionItemEntity),
                        ciop.CollectionItemEntity.CollectionItemEntityID,
                        nameof(CollectionItemEntity.InscriptionTranslated),
                        ciop.CollectionItemEntity.Inscription) ?? ciop.CollectionItemEntity.InscriptionTranslated;
                }
                if(!string.IsNullOrEmpty(ciop.CollectionItemEntity.Comment))
                {
                    ciop.CollectionItemEntity.Comment = translationStore.GetTranslation(
                        nameof(CollectionItemEntity),
                        ciop.CollectionItemEntity.CollectionItemEntityID,
                        nameof(CollectionItemEntity.Comment),
                        ciop.CollectionItemEntity.Comment) ?? ciop.CollectionItemEntity.Comment;
                }
                if (!string.IsNullOrEmpty(ciop.Era.EraName))
                {
                    ciop.Era.EraName = translationStore.GetTranslation(
                    nameof(CollectionItemEntity),
                    ciop.CollectionItemEntity.CollectionItemEntityID,
                    nameof(Era.EraName),
                    ciop.Era.EraName) ?? ciop.Era.EraName;
                }
                ciop.CollectionItemEntity.StatePreservation?.StatePreservationName = translationStore.GetTranslation(
                                nameof(CollectionItemEntity),
                                ciop.CollectionItemEntity.CollectionItemEntityID,
                                nameof(StatePreservation.StatePreservationName),
                                ciop.CollectionItemEntity.StatePreservation.StatePreservationName) ?? ciop.CollectionItemEntity.StatePreservation.StatePreservationName;
                foreach(var cav in ciop.ConceptValueList)
                {
                    if (!string.IsNullOrEmpty(cav.ValueString))
                    {
                        cav.ValueString = translationStore.GetTranslation(
                            nameof(CollectionItemEntity),
                            ciop.CollectionItemEntity.CollectionItemEntityID,
                            nameof(ConceptValue.ValueString),
                            cav.ValueString) ?? cav.ValueString;
                    }
                }
                foreach (var placeConnection in ciop.CollectionItemNPlaceList)
                {
                    foreach(var toponymyConnection in placeConnection.Place?.PlaceNToponymyList ?? [])
                    {
                        toponymyConnection.Toponymy.ToponymyName = translationStore.GetTranslation(
                            nameof(CollectionItemEntity),
                            ciop.CollectionItemEntity.CollectionItemEntityID,
                            nameof(Toponymy.ToponymyName),
                            toponymyConnection.Toponymy.ToponymyName) ?? toponymyConnection.Toponymy.ToponymyName;
                    }
                }
            }

            return ciopList;
        }

        public List<CollectionItemOperationParameterModel> GetWithVector(CollectionItemSearchParameterModel model)
        {
            if (string.IsNullOrEmpty(model.SemanticSearchQuery))
                return GetWithPredicates(model);

            var vectorResults = processCollectionItemEmbedding.Search(model.SemanticSearchQuery);

            //// Filtern nach Minimum Similarity Score
            //if (model.MinimumSimilarityScore.HasValue)
            //{
            //    vectorResults = [.. vectorResults.Where(x => x.SimilarityScore >= model.MinimumSimilarityScore.Value)];
            //}
            if(vectorResults.Count == 0)
            {
                return [];
            }

            model.CollectionItemEntityID = [.. vectorResults.Select(x => x.CollectionItemEntityID)];
            return GetWithPredicates(model);
        }

        private List<string> ConnectPartyToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int partyID, string? relationship)
        {
            if (partyID <= 0)
            {
                return [];
            }
            Party? party = unitOfWork.PartyRepository.GetByID(partyID);
            if (party is null)
            {
                return [];
            }

            CollectionItemNParty collectionItemEntityNParty = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                PartyID = partyID,
                Relationship = relationship ?? string.Empty
            };
            _ = unitOfWork.CollectionItemNPartyRepository.Insert(collectionItemEntityNParty);
            unitOfWork.Save();

            return translationStore.GetById<Party>(partyID);
        }
        private List<string> SyncPartyConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNParty> newConnections)
        {
            List<CollectionItemNParty> currentConnections = existingCollectionItemEntity.CollectionItemNPartyList;

            foreach (CollectionItemNParty? current in currentConnections)
            {
                CollectionItemNParty? updated = newConnections.FirstOrDefault(x => x.PartyID == current.PartyID);

                if (updated == null)
                {
                    DisconnectPartyConnection(existingCollectionItemEntity, current.PartyID);
                }
                else if (updated is not null && updated.Relationship != current.Relationship)
                {
                    UpdateCollectionItemNParty(existingCollectionItemEntity, updated);
                }
            }

            List<string> translationList = [];
            foreach (CollectionItemNParty newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PartyID == newItem.PartyID);
                if (!exists)
                {
                    translationList.AddRange(ConnectPartyToCollectionItemEntity(existingCollectionItemEntity, newItem.PartyID, newItem.Relationship));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNParty(CollectionItemEntity existingCollectionItemEntity, CollectionItemNParty updated)
        {
            CollectionItemNParty? collectionItemNParty = (from bep in unitOfWork.CollectionItemNPartyRepository.Get(includeProperties: "Party")
                                                          where bep.PartyID == updated.PartyID && bep.CollectionItemEntity == existingCollectionItemEntity
                                                          select bep).FirstOrDefault();

            if (collectionItemNParty != null)
            {
                collectionItemNParty.Relationship = updated.Relationship;
                unitOfWork.Save();
            }
        }
        private void DisconnectPartyConnection(CollectionItemEntity collectionItemEntity, int personID)
        {
            if (collectionItemEntity.CollectionItemEntityID > 0 && personID > 0)
            {
                CollectionItemNParty? collectionItemNParty = (from bep in unitOfWork.CollectionItemNPartyRepository.Get(includeProperties: "Party")
                                                              where bep.PartyID == personID && bep.CollectionItemEntity == collectionItemEntity
                                                              select bep).FirstOrDefault();

                if (collectionItemNParty != null)
                {
                    unitOfWork.CollectionItemNPartyRepository.Delete(collectionItemNParty);
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

            CollectionItemNPlace collectionItemEntityNPlace = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                PlaceID = placeID,
                Relationship = relationship
            };
            _ = unitOfWork.CollectionItemNPlaceRepository.Insert(collectionItemEntityNPlace);
            unitOfWork.Save();

            return translationStore.GetById<Place>(placeID);
        }
        private List<string> SyncPlaceConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNPlace> newConnections)
        {
            List<CollectionItemNPlace> currentConnections = existingCollectionItemEntity.CollectionItemNPlaceList;

            foreach (CollectionItemNPlace? current in currentConnections)
            {
                CollectionItemNPlace? updated = newConnections.FirstOrDefault(x => x.PlaceID == current.PlaceID);

                if (updated == null)
                {
                    DisconnectPlaceConnection(existingCollectionItemEntity, current.PlaceID);
                }
                else if (updated is not null && updated.Relationship != current.Relationship)
                {
                    UpdateCollectionItemNPlace(existingCollectionItemEntity, updated);
                }
            }

            List<string> translationList = [];
            foreach (CollectionItemNPlace newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.PlaceID);
                if (!exists)
                {
                    translationList.AddRange(ConnectPlaceToCollectionItemEntity(existingCollectionItemEntity, newItem.PlaceID, newItem.Relationship));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNPlace(CollectionItemEntity existingCollectionItemEntity, CollectionItemNPlace updated)
        {
            CollectionItemNPlace? collectionItemNPlace = (from bec in unitOfWork.CollectionItemNPlaceRepository.Get(includeProperties: "Place")
                                                          where bec.PlaceID == updated.PlaceID && bec.CollectionItemEntity == existingCollectionItemEntity
                                                          select bec).FirstOrDefault();
            if (collectionItemNPlace != null)
            {
                collectionItemNPlace.Relationship = updated.Relationship;
                unitOfWork.Save();
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

        private (List<(CollectionItemPicture CollectionItemPicture, int PictureId, string Process)>, int Statuscode, string Statusmessage) SyncPictureConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemPicture> newConnections)
        {
            List<CollectionItemPicture> currentConnections = existingCollectionItemEntity.CollectionItemPictureList;
            List<(CollectionItemPicture CollectionItemPicture, int PictureId, string Process)> pictureResults = [];

            for (int i = 0; i < currentConnections.Count; i++)
            {
                CollectionItemPicture? updated = newConnections.FirstOrDefault(x => x.CollectionItemPictureID == currentConnections[i].CollectionItemPictureID);

                if (updated == null)
                {
                    (int statusCode, string statusMessage) = processCollectionItemPicture.Delete(currentConnections[i]);
                    if (statusCode != 200)
                    {
                        return ([], statusCode, statusMessage);
                    }
                    pictureResults.Add((currentConnections[i], currentConnections[i].CollectionItemPictureID, "delete"));
                }
                else if (updated != null)
                {
                    (int statusCode, string statusMessage) = processCollectionItemPicture.Update(updated, existingCollectionItemEntity);
                    if (statusCode != 200)
                    {
                        return ([], statusCode, statusMessage);
                    }
                    if (updated.IFormFile != null)
                        pictureResults.Add((updated, updated.CollectionItemPictureID, "update"));
                }
            }

            foreach (CollectionItemPicture newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.CollectionItemPictureID == newItem.CollectionItemPictureID);
                if (!exists)
                {
                    (int statusCode, string statusMessage, int pictureId) = processCollectionItemPicture.Insert(newItem, existingCollectionItemEntity);
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
