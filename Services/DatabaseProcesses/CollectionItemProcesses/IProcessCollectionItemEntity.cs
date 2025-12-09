using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEntity
    {
        List<CollectionItemOperationParameterModel> GetWithPredicates(CollectionItemSearchParameterModel model); 
        List<CollectionItemOperationParameterModel> GetWithVector(CollectionItemSearchParameterModel model);
        List<CollectionItemOperationParameterModel> GetTraditionalTextSearch(CollectionItemSearchParameterModel model);
        string Insert(CollectionItemOperationParameterModel model);
        string Update(CollectionItemOperationParameterModel model);
        string Delete(CollectionItemOperationParameterModel model);
    }

    public class CollectionItemEntityProcessor(IUnitOfWork unitOfWork,
        IProcessCollectionItemPicture processCollectionItemPicture,
        IProcessPicturePhysically processPicturePhysically,
        IProcessCollectionAttributeValue processCollectionAttributeValue,
        IProcessCollectionItemPotential processCollectionItemPotential,
        IProcessCollectionItemEmbedding processCollectionItemEmbedding,
        DeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore) : IProcessCollectionItemEntity
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

        public string Insert(CollectionItemOperationParameterModel model)
        {
            if (model.CollectionItemEntity.CollectionAreaID <= 0)
            {
                return "Error_CollectionAreaID_Missing";
            }
            if (string.IsNullOrEmpty(model.CollectionItemEntity.UsingIdentityUsersID))
            {
                return "Error_UserID_Missing";
            }

            List<(CollectionItemPicture, int)> pictureList = [];

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (!string.IsNullOrEmpty(model.CollectionItemEntity.Comment)) 
                {
                    model.CollectionItemEntity.Comment = translationService.SetIntoFallbackLanguage(model.CollectionItemEntity.Comment);
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.Inscription))
                {
                    model.CollectionItemEntity.InscriptionTranslated = translationService.SetIntoFallbackLanguage(model.CollectionItemEntity.Inscription);
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.UniqueName))
                {
                    model.CollectionItemEntity.UniqueName = translationService.SetIntoFallbackLanguage(model.CollectionItemEntity.UniqueName);
                }

                CollectionItemEntity newCollectionItemEntity = unitOfWork.CollectionItemEntityRepository.Insert(model.CollectionItemEntity);
                unitOfWork.Save();

                List<string> translationList = [];
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.Comment))
                {
                    translationList.AddRange([.. processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Comment),
                        TranslatedText = newCollectionItemEntity.Comment ?? string.Empty,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)

                    },
                    model.CollectionItemEntity.Comment).Values]);
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.InscriptionTranslated))
                {
                    translationList.AddRange([.. processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.Inscription),
                        TranslatedText = newCollectionItemEntity.Inscription ?? string.Empty,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    model.CollectionItemEntity.InscriptionTranslated).Values]);
                }
                if (!string.IsNullOrEmpty(model.CollectionItemEntity.UniqueName))
                {
                    translationList.AddRange([.. processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionItemEntity),
                        EntityId = newCollectionItemEntity.CollectionItemEntityID,
                        FieldName = nameof(CollectionItemEntity.UniqueName),
                        TranslatedText = newCollectionItemEntity.UniqueName ?? string.Empty,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    model.CollectionItemEntity.UniqueName).Values]);
                }

                if (model.IsPartOfASeries)
                {
                    if (model.CollectionItemEntity.CollectionItemPotentialID > 0)
                    {
                        newCollectionItemEntity.CollectionItemPotentialID = model.CollectionItemEntity.CollectionItemPotentialID;
                    }
                    else
                    {
                        newCollectionItemEntity.CollectionItemPotentialID = processCollectionItemPotential.Create().CollectionItemPotentialID;
                    }
                    unitOfWork.Save();
                }

                foreach (CollectionItemNParty entityNParty in model.CollectionItemNPartyList)
                {
                    translationList.AddRange(ConnectPartyToCollectionItemEntity(newCollectionItemEntity, entityNParty.PartyID, entityNParty.Relationship));
                }
                foreach (CollectionItemNPlace collectionItemEntityNPlace in model.CollectionItemNPlaceList)
                {
                    translationList.AddRange(ConnectPlaceToCollectionItemEntity(newCollectionItemEntity, collectionItemEntityNPlace.PlaceID, collectionItemEntityNPlace.Relationship));
                }
                foreach (CollectionItemNColor collectionItemNColor in model.CollectionItemNColorList)
                {
                    translationList.AddRange(ConnectColorToCollectionItemEntity(newCollectionItemEntity, collectionItemNColor));
                }
                foreach (CollectionItemNMaterial collectionItemNMaterial in model.CollectionItemNMaterialList)
                {
                    translationList.AddRange(ConnectMaterialToCollectionItemEntity(newCollectionItemEntity, collectionItemNMaterial));
                }
                foreach (CollectionAttributeValue collectionAttributeValue in model.CollectionAttributeValueList)
                {
                    (int code, string returnMessage, translationList) = processCollectionAttributeValue.Insert(collectionAttributeValue, newCollectionItemEntity.CollectionItemEntityID);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (returnMessage);
                    }
                }

                (int statuscode, string statusmessage) =  processCollectionItemEmbedding.Insert(newCollectionItemEntity, translationList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
                }

                foreach (CollectionItemPicture collectionItemPicture in model.CollectionItemPictureList)
                {
                    (int pictureId, int code, string returnMessage) = processCollectionItemPicture.Insert(collectionItemPicture, newCollectionItemEntity);
                    pictureList.Add((collectionItemPicture, pictureId));
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (returnMessage);
                    }
                }
                foreach (var picture in pictureList)
                {
                    (int code, string returnMessage) = processPicturePhysically.Save(picture.Item1, picture.Item2, false);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (returnMessage);
                    }
                }

                scope.Complete();

                return "Success_CollectionItemEntity_Created";
            }
            catch (Exception ex)
            {
                return "Error_Error_Ocurred";
            }
        }

        public string Update(CollectionItemOperationParameterModel operationModel)
        {
            CollectionItemSearchParameterModel collectionItemSearchParameterModel = new();
            collectionItemSearchParameterModel.CollectionItemEntityID.Add(operationModel.CollectionItemEntity.CollectionItemEntityID);
            CollectionItemEntity? existingEntity = GetWithPredicates(collectionItemSearchParameterModel).FirstOrDefault()?.CollectionItemEntity;
            if (existingEntity == null)
            {
                return "Error_CollectionItemEntity_NotFound";
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
                translationList.AddRange(SyncColorConnections(existingEntity, operationModel.CollectionItemNColorList));
                translationList.AddRange(SyncMaterialConnections(existingEntity, operationModel.CollectionItemNMaterialList));
                (statuscode, statusmessage, translationList) = SyncCollectionAttributeValueConnections(existingEntity, operationModel.CollectionAttributeValueList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
                }
                (statuscode, statusmessage) = processCollectionItemEmbedding.Update(existingEntity, translationList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
                }

                (List<(CollectionItemPicture collectionItemPicture, int pictureId, string process)> pictureList, statuscode, statusmessage) = SyncPictureConnections(existingEntity, operationModel.CollectionItemPictureList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
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
                        return (statusmessage);
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
                        return (statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "delete"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Delete(pictureId); 
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statusmessage);
                    }
                }

                scope.Complete();

                return "Success_CollectionItemEntity_Changed";
            }
            catch (Exception ex)
            {
                return "Error_Error_Ocurred";
            }

            List<string> ChangeEntity(CollectionItemOperationParameterModel operationModel, CollectionItemEntity existingEntity)
            {
                bool hasChanges = false;
                List<string> translationList = [];

                if (existingEntity.CollectionItemPotentialID != operationModel.CollectionItemEntity.CollectionItemPotentialID)
                {
                    existingEntity.CollectionItemPotentialID = operationModel.CollectionItemEntity.CollectionItemPotentialID;
                    hasChanges = true;
                }
                if (existingEntity.Comment != operationModel.CollectionItemEntity.Comment && !string.IsNullOrEmpty(operationModel.CollectionItemEntity.Comment))
                {
                    existingEntity.Comment = translationService.SetIntoFallbackLanguage(operationModel.CollectionItemEntity.Comment);
                    translationList.AddRange([.. processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionItemEntity),
                            EntityId = existingEntity.CollectionItemEntityID,
                            FieldName = nameof(CollectionItemEntity.Comment),
                            TranslatedText = existingEntity.Comment ?? string.Empty,
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                        },
                        operationModel.CollectionItemEntity.Comment).Values]);
                    hasChanges = true;
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
                if (existingEntity.StateID != operationModel.CollectionItemEntity.StateID)
                {
                    existingEntity.StateID = operationModel.CollectionItemEntity.StateID;
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
                if (existingEntity.UniqueName != operationModel.CollectionItemEntity.UniqueName && !string.IsNullOrEmpty(operationModel.CollectionItemEntity.UniqueName))
                {
                    existingEntity.UniqueName = translationService.SetIntoFallbackLanguage(operationModel.CollectionItemEntity.UniqueName);
                    translationList.AddRange([.. processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionItemEntity),
                            EntityId = existingEntity.CollectionItemEntityID,
                            FieldName = nameof(CollectionItemEntity.UniqueName),
                            TranslatedText = existingEntity.UniqueName ?? string.Empty,
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                        },
                        operationModel.CollectionItemEntity.UniqueName).Values]);
                    hasChanges = true;
                }
                if (existingEntity.Inscription != operationModel.CollectionItemEntity.Inscription)
                {
                    existingEntity.Inscription = operationModel.CollectionItemEntity.Inscription;
                    hasChanges = true;
                }
                if (existingEntity.Inscription != operationModel.CollectionItemEntity.InscriptionTranslated && !string.IsNullOrEmpty(operationModel.CollectionItemEntity.Inscription))
                {
                    existingEntity.Inscription = translationService.SetIntoFallbackLanguage(operationModel.CollectionItemEntity.Inscription);
                    translationList.AddRange([.. processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionItemEntity),
                            EntityId = existingEntity.CollectionItemEntityID,
                            FieldName = nameof(CollectionItemEntity.Inscription),
                            TranslatedText = existingEntity.Inscription ?? string.Empty,
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                        },
                        operationModel.CollectionItemEntity.Inscription).Values]);
                    hasChanges = true;
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

        public string Delete(CollectionItemOperationParameterModel model)
        {
            CollectionItemOperationParameterModel? existingOperationParameterModel = GetWithPredicates(ParametersOperationToSearch(model)).FirstOrDefault();
            if (existingOperationParameterModel == null)
            {
                return "Error_CollectionItemEntity_NotFound";
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
                for (int i = existingOperationParameterModel.CollectionItemNColorList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectColorConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNColorList[index].ColorID);
                }
                for (int i = existingOperationParameterModel.CollectionItemNMaterialList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectMaterialConnection(existingOperationParameterModel.CollectionItemEntity, existingOperationParameterModel.CollectionItemNMaterialList[index].MaterialID);
                }
                for (int i = existingOperationParameterModel.CollectionItemPictureList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    picutureIdList.Add(existingOperationParameterModel.CollectionItemPictureList[index].CollectionItemPictureID);
                    (int statuscode, string statusmessage) = processCollectionItemPicture.Delete(existingOperationParameterModel.CollectionItemPictureList[index]);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return statusmessage;
                    }
                }
                for (int i = existingOperationParameterModel.CollectionAttributeValueList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    (int statuscode, string statusmessage) = processCollectionAttributeValue.Delete(existingOperationParameterModel.CollectionAttributeValueList[index].CollectionAttributeValueID);
                    if (statuscode != 200)
                    {
                        scope.Dispose();
                        return statusmessage;
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
                        return statusmessage;
                    }
                }

                scope.Complete();

                return "Success_CollectionItemEntity_Deleted";
            }
            catch (Exception ex)
            {
                return "Error_Error_Ocurred";
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
            if (string.IsNullOrEmpty(model.SemanticSearchQuery))
                return GetWithPredicates(model);

            var searchTerms = model.SemanticSearchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
            var results = unitOfWork.CollectionItemEntityRepository.Get(
                includeProperties: GetIncludeProperties(),
                filter: item =>
                    searchTerms.Any(term =>
                        (item.CollectionArea != null && item.CollectionArea.CollectionAreaName.Contains(term)) ||
                        (item.CollectionAttributeValueList.Any(p =>
                            p.CollectionAttribute != null && p.CollectionAttribute.CollectionAttributeName.Contains(term))) ||
                        (item.State != null && item.State.StateName.Contains(term)) ||
                        (item.SerialNumber != null && item.SerialNumber.Contains(term)) ||
                        (item.PersonalIdentificationNumber != null && item.PersonalIdentificationNumber.Contains(term)) ||
                        (item.UniqueName != null && item.UniqueName.Contains(term)) ||
                        (item.Comment != null && item.Comment.Contains(term)) ||
                        (item.Inscription != null && item.Inscription.Contains(term)) ||
                        (item.ExactYear != null && item.ExactYear != null && item.ExactYear.ToString().Contains(term)) ||
                        (item.StartYear != null && item.StartYear != null && item.StartYear.ToString().Contains(term)) ||
                        (item.EndYear != null && item.EndYear != null && item.EndYear.ToString().Contains(term)) ||
                        item.CollectionItemNPlaceList.Any(p =>
                            p.Place != null && p.Place.PlaceNToponymyList.Any(t => t.Toponymy.ToponymyName.Contains(term))) ||
                        item.CollectionItemNPartyList.Any(p =>
                            p.Party != null && p.Party.PartyName.Contains(term)) ||
                        item.CollectionItemNMaterialList.Any(m =>
                            m.Material != null && m.Material.MaterialName.Contains(term)) ||
                        item.CollectionItemNColorList.Any(m =>
                            m.Color != null && m.Color.Name.Contains(term)) ||
                        (item.Concept != null && item.Concept.ConceptName.Contains(term)) ||
                        (item.Era != null && item.Era.EraName.Contains(term))
                    )
            );
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.

            return [.. from b in results
               select SetMembersofEntity(b)];
        }
        private static string GetIncludeProperties()
        {
            return "CollectionItemPictureList," +
                   "UsingIdentityUser," +
                   "Concept," +
                   "CollectionItemNColorList.Color," +
                   "CollectionItemPotential," +
                   "CollectionItemNMaterialList.Material," +
                   "CollectionAttributeValueList.CollectionAttribute," +
                   "CollectionItemNPlaceList.Place.PlaceNToponymyList.Toponymy," +
                   "CollectionItemNPlaceList.Place.Settlement.SettlementNPostalcodeList.Postalcode," +
                   "CollectionItemNPartyList.Party," +
                   "Era," +
                   "State";
        }
        private CollectionItemOperationParameterModel SetMembersofEntity(CollectionItemEntity b)
        {
            return new CollectionItemOperationParameterModel
            {
                CollectionItemEntity = b,
                CollectionItemPotential = b.CollectionItemPotential ?? new(),
                CollectionItemPictureList = b.CollectionItemPictureList,
                CollectionItemNColorList = b.CollectionItemNColorList,
                CollectionItemNMaterialList = b.CollectionItemNMaterialList,
                StateList = [.. unitOfWork.StateRepository.Get()],
                ColorList = [.. unitOfWork.ColorRepository.Get()],
                MaterialList = [.. unitOfWork.MaterialRepository.Get()],
                CollectionAttributeList = [.. unitOfWork.CollectionAttributeRepository.Get(filter: ca => ca.CollectionAreaID == b.CollectionAreaID)],
                CollectionAttributeValueList = b.CollectionAttributeValueList,
                CollectionItemNPartyList = b.CollectionItemNPartyList,
                CollectionItemNPlaceList = b.CollectionItemNPlaceList,
                Concept = b.Concept ?? new() { ConceptName = string.Empty },
                Era = b.Era ?? new() { EraName = string.Empty },
                IsPartOfASeries = b.CollectionItemPotential?.CollectionItemPotentialID > 0
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
                if (!string.IsNullOrEmpty(ciop.CollectionItemEntity.InscriptionTranslated))
                {
                    ciop.CollectionItemEntity.InscriptionTranslated = translationStore.GetTranslation(
                        nameof(CollectionItemEntity),
                        ciop.CollectionItemEntity.CollectionItemEntityID,
                        nameof(CollectionItemEntity.InscriptionTranslated),
                        ciop.CollectionItemEntity.InscriptionTranslated) ?? ciop.CollectionItemEntity.InscriptionTranslated;
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
                ciop.Concept.ConceptName = translationStore.GetTranslation(
                    nameof(CollectionItemEntity),
                    ciop.CollectionItemEntity.CollectionItemEntityID,
                    nameof(Concept.ConceptName),
                    ciop.Concept.ConceptName) ?? ciop.Concept.ConceptName;
                if (ciop.CollectionItemEntity.State != null)
                {
                    ciop.CollectionItemEntity.State.StateName = translationStore.GetTranslation(
                        nameof(CollectionItemEntity),
                        ciop.CollectionItemEntity.CollectionItemEntityID,
                        nameof(State.StateName),
                        ciop.CollectionItemEntity.State.StateName) ?? ciop.CollectionItemEntity.State.StateName;
                }
                foreach(var cav in ciop.CollectionAttributeValueList)
                {
                    if (!string.IsNullOrEmpty(cav.ValueString))
                    {
                        cav.ValueString = translationStore.GetTranslation(
                            nameof(CollectionItemEntity),
                            ciop.CollectionItemEntity.CollectionItemEntityID,
                            nameof(CollectionAttributeValue.ValueString),
                            cav.ValueString) ?? cav.ValueString;
                    }
                }
                foreach (var placeConnection in ciop.CollectionItemNPlaceList)
                {
                    foreach(var toponymyConnection in placeConnection.Place?.PlaceNToponymyList ?? [])
                    {
                        if (toponymyConnection.Toponymy != null && !string.IsNullOrEmpty(toponymyConnection.Toponymy.ToponymyName))
                        {
                            toponymyConnection.Toponymy.ToponymyName = translationStore.GetTranslation(
                                nameof(CollectionItemEntity),
                                ciop.CollectionItemEntity.CollectionItemEntityID,
                                nameof(Toponymy.ToponymyName),
                                toponymyConnection.Toponymy.ToponymyName) ?? toponymyConnection.Toponymy.ToponymyName;
                        }
                    }
                }
                foreach (var materialConnection in ciop.CollectionItemNMaterialList)
                {
                    if (materialConnection.Material != null && !string.IsNullOrEmpty(materialConnection.Material.MaterialName))
                    {
                        materialConnection.Material.MaterialName = translationStore.GetTranslation(
                            nameof(CollectionItemEntity),
                            ciop.CollectionItemEntity.CollectionItemEntityID,
                            nameof(Material.MaterialName),
                            materialConnection.Material.MaterialName) ?? materialConnection.Material.MaterialName;
                    }
                }
                foreach (var colorConnection in ciop.CollectionItemNColorList)
                {
                    if (colorConnection.Color != null && !string.IsNullOrEmpty(colorConnection.Color.Name))
                    {
                        colorConnection.Color.Name = translationStore.GetTranslation(
                            nameof(CollectionItemEntity),
                            ciop.CollectionItemEntity.CollectionItemEntityID,
                            nameof(Color.Name),
                            colorConnection.Color.Name) ?? colorConnection.Color.Name;
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

        private List<string> ConnectPlaceToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int placeID, string? relationship)
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

        private List<string> ConnectColorToCollectionItemEntity(CollectionItemEntity collectionItem, CollectionItemNColor collectionItemNColor)
        {
            if (collectionItemNColor.ColorID <= 0)
            {
                return [];
            }

            Color? color = unitOfWork.ColorRepository.GetByID(collectionItemNColor.ColorID);
            if (color is null)
            {
                return [];
            }

            collectionItemNColor.CollectionItemEntityID = collectionItem.CollectionItemEntityID;

            _ = unitOfWork.CollectionItemNColorRepository.Insert(collectionItemNColor);
            unitOfWork.Save();

            return translationStore.GetById<Color>(collectionItemNColor.ColorID);
        }
        private List<string> SyncColorConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNColor> newConnections)
        {
            List<CollectionItemNColor> currentConnections = existingCollectionItemEntity.CollectionItemNColorList;

            foreach (CollectionItemNColor? current in currentConnections)
            {
                CollectionItemNColor? updated = newConnections.FirstOrDefault(x => x.ColorID == current.ColorID);

                if (updated == null)
                {
                    DisconnectColorConnection(existingCollectionItemEntity, current.ColorID);
                }
                else if (updated is not null && (updated.IsPrimaryColor != current.IsPrimaryColor))
                {
                    UpdateCollectionItemNColor(existingCollectionItemEntity, updated);
                }
            }

            List<string> translationList = [];
            foreach (CollectionItemNColor newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ColorID == newItem.ColorID);
                if (!exists)
                {
                    translationList.AddRange(ConnectColorToCollectionItemEntity(existingCollectionItemEntity, newItem));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNColor(CollectionItemEntity existingCollectionItemEntity, CollectionItemNColor updated)
        {
            CollectionItemNColor? collectionItemNColor = (from pnc in unitOfWork.CollectionItemNColorRepository.Get(includeProperties: "Color")
                                                                        where pnc.ColorID == updated.ColorID && pnc.CollectionItemEntity == existingCollectionItemEntity
                                                                        select pnc).FirstOrDefault();

            if (collectionItemNColor != null)
            {
                collectionItemNColor.IsPrimaryColor = updated.IsPrimaryColor;
                unitOfWork.Save();
            }
        }
        private void DisconnectColorConnection(CollectionItemEntity collectionItemEntity, int colorID)
        {
            if (collectionItemEntity.CollectionItemEntityID > 0 && colorID > 0)
            {
                CollectionItemNColor? collectionItemNColor = (from pnc in unitOfWork.CollectionItemNColorRepository.Get(includeProperties: "Color")
                                                                            where pnc.ColorID == colorID && pnc.CollectionItemEntity == collectionItemEntity
                                                                            select pnc).FirstOrDefault();

                if (collectionItemNColor != null)
                {
                    unitOfWork.CollectionItemNColorRepository.Delete(collectionItemNColor);
                    unitOfWork.Save();
                }
            }
        }

        private List<string> ConnectMaterialToCollectionItemEntity(CollectionItemEntity collectionItem, CollectionItemNMaterial collectionItemNMatreial)
        {
            if (collectionItemNMatreial.MaterialID <= 0)
            {
                return [];
            }

            Material? material = unitOfWork.MaterialRepository.GetByID(collectionItemNMatreial.MaterialID);
            if (material is null)
            {
                return [];
            }

            collectionItemNMatreial.CollectionItemEntityID = collectionItem.CollectionItemEntityID;

            _ = unitOfWork.CollectionItemNMaterialRepository.Insert(collectionItemNMatreial);
            unitOfWork.Save();

            return translationStore.GetById<Material>(collectionItemNMatreial.MaterialID);
        }
        private List<string> SyncMaterialConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNMaterial> newConnections)
        {
            List<CollectionItemNMaterial> currentConnections = existingCollectionItemEntity.CollectionItemNMaterialList;

            foreach (CollectionItemNMaterial? current in currentConnections)
            {
                CollectionItemNMaterial? updated = newConnections.FirstOrDefault(x => x.MaterialID == current.MaterialID);

                if (updated == null)
                {
                    DisconnectMaterialConnection(existingCollectionItemEntity, current.MaterialID);
                }
                else if (updated is not null && updated.IsPrimaryMaterial != current.IsPrimaryMaterial)
                {
                    UpdateCollectionItemNMaterial(existingCollectionItemEntity, updated);
                }
            }

            List<string> translationList = [];
            foreach (CollectionItemNMaterial newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.MaterialID == newItem.MaterialID);
                if (!exists)
                {
                    translationList.AddRange(ConnectMaterialToCollectionItemEntity(existingCollectionItemEntity, newItem));
                }
            }

            return translationList;
        }
        private void UpdateCollectionItemNMaterial(CollectionItemEntity existingCollectionItemEntity, CollectionItemNMaterial updated)
        {
            CollectionItemNMaterial? collectionItemNMaterial = (from pnm in unitOfWork.CollectionItemNMaterialRepository.Get(includeProperties: "Material")
                                                                where pnm.MaterialID == updated.MaterialID && pnm.CollectionItemEntity == existingCollectionItemEntity
                                                                select pnm).FirstOrDefault();

            if (collectionItemNMaterial != null)
            {
                collectionItemNMaterial.IsPrimaryMaterial = updated.IsPrimaryMaterial;
                unitOfWork.Save();
            }
        }
        private void DisconnectMaterialConnection(CollectionItemEntity collectionItem, int materialID)
        {
            if (collectionItem.CollectionItemEntityID <= 0 && materialID <= 0)
            {
                return;
            }

            CollectionItemNMaterial? collectionItemNMaterial = (from pnm in unitOfWork.CollectionItemNMaterialRepository.Get(includeProperties: "Material")
                                                                where pnm.MaterialID == materialID && pnm.CollectionItemEntity == collectionItem
                                                                select pnm).FirstOrDefault();

            if (collectionItemNMaterial != null)
            {
                unitOfWork.CollectionItemNMaterialRepository.Delete(collectionItemNMaterial);
                unitOfWork.Save();
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
                    (int newCollectionItemPicInt, int statusCode, string statusMessage) = processCollectionItemPicture.Insert(newItem, existingCollectionItemEntity);
                    if (statusCode != 200)
                    {
                        return ([], statusCode, statusMessage);
                    }
                    pictureResults.Add((newItem, newCollectionItemPicInt, "insert"));
                }
            }
            return (pictureResults, 200, "Success_CollectionItemPicture_Synchronized");
        }

        private (int Statuscode, string Statusmessage, List<string> TranslationList) SyncCollectionAttributeValueConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionAttributeValue> newConnections)
        {
            List<CollectionAttributeValue> currentConnections = existingCollectionItemEntity.CollectionAttributeValueList;
            int statusCode;
            string statusMessage;
            List<string> translationList = [];

            for (int i = 0; i < currentConnections.Count; i++)
            {
                CollectionAttributeValue? updated = newConnections.FirstOrDefault(x => x.CollectionAttributeValueID == currentConnections[i].CollectionAttributeValueID);

                if (updated == null)
                {
                    (statusCode, statusMessage) = processCollectionAttributeValue.Delete(currentConnections[i].CollectionAttributeValueID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
                else if (updated != null)
                {
                    (statusCode, statusMessage, translationList) = processCollectionAttributeValue.Update(updated);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
            }

            foreach (CollectionAttributeValue newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.CollectionAttributeID == newItem.CollectionAttributeID);
                if (!exists)
                {
                    (statusCode, statusMessage, translationList) = processCollectionAttributeValue.Insert(newItem, existingCollectionItemEntity.CollectionItemEntityID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage, translationList);
                    }
                }
            }
            return (200, "Success_CollectionAttributeValue_Synchronized", translationList);
        }
    }
}
