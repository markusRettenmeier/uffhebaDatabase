using Microsoft.CodeAnalysis;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Services.Processes.PictureProcesses;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.CollectionItemProcesses
{
    public interface IProcessCollectionItemEntity
    {
        List<CollectionItemOperationParameterModel> GetWithPredicates(CollectionItemSearchParameterModel model);
        string Insert(CollectionItemOperationParameterModel model);
        string Update(CollectionItemOperationParameterModel model);
        string Delete(CollectionItemOperationParameterModel model);
        CollectionItemSearchParameterModel ParametersOperationToSearch(CollectionItemOperationParameterModel model);
    }

    public class CollectionItemEntityProcessor(IUnitOfWork unitOfWork,
        IProcessCollectionItemPicture processCollectionItemPicture,
        IProcessPicturePhysically processPicturePhysically,
        IProcessCollectionItemValue processCollectionItemValue,
        IProcessCollectionItemPotential processCollectionItemPotential) : IProcessCollectionItemEntity
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
                return ("Ungültige CollectionAreaID.");
            }
            if (string.IsNullOrEmpty(model.CollectionItemEntity.UsingIdentityUsersID))
            {
                return ("Ungültige UserID.");
            }

            List<(CollectionItemPicture, int)> pictureList = [];

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionItemEntity newCollectionItemEntity = unitOfWork.CollectionItemEntityRepository.Insert(model.CollectionItemEntity);
                unitOfWork.Save();

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
                    ConnectPartyToCollectionItemEntity(newCollectionItemEntity, entityNParty.PartyID, entityNParty.Relationship);
                }
                foreach (CollectionItemNPlace collectionItemEntityNPlace in model.CollectionItemNPlaceList)
                {
                    ConnectPlaceToCollectionItemEntity(newCollectionItemEntity, collectionItemEntityNPlace.PlaceID, collectionItemEntityNPlace.Relationship);
                }
                foreach (CollectionItemNColor collectionItemNColor in model.CollectionItemNColorList)
                {
                    ConnectColorToCollectionItemEntity(newCollectionItemEntity, collectionItemNColor);
                }
                foreach (CollectionItemNMaterial collectionItemNMaterial in model.CollectionItemNMaterialList)
                {
                    ConnectMaterialToCollectionItemEntity(newCollectionItemEntity, collectionItemNMaterial);
                }
                foreach (CollectionItemValue collectionItemValue in model.CollectionItemValueList)
                {
                    (int code, string returnMessage) = processCollectionItemValue.Insert(collectionItemValue, newCollectionItemEntity.CollectionItemEntityID);
                    if (code != 200)
                    {
                        scope.Dispose();
                        return (returnMessage);
                    }
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

                return ("Sammlerstück wurde erstellt.");
            }
            catch (Exception ex)
            {
                return ("Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public string Update(CollectionItemOperationParameterModel operationModel)
        {
            CollectionItemSearchParameterModel collectionItemSearchParameterModel = new();
            collectionItemSearchParameterModel.CollectionItemEntityID.Add(operationModel.CollectionItemEntity.CollectionItemEntityID);
            CollectionItemEntity? existingEntity = GetWithPredicates(collectionItemSearchParameterModel).FirstOrDefault()?.CollectionItemEntity;
            if (existingEntity == null)
            {
                return ("Sammlerstück nicht gefunden.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                ChangeEntity(operationModel, existingEntity);

                SyncPartyConnections(existingEntity, operationModel.CollectionItemNPartyList);
                SyncPlaceConnections(existingEntity, operationModel.CollectionItemNPlaceList);
                SyncColorConnections(existingEntity, operationModel.CollectionItemNColorList);
                SyncMaterialConnections(existingEntity, operationModel.CollectionItemNMaterialList);
                (List<(CollectionItemPicture collectionItemPicture, int pictureId, string process)> pictureList, int statuscode, string statusmessage) = SyncPictureConnections(existingEntity, operationModel.CollectionItemPictureList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
                }
                (statuscode, statusmessage) = SyncCollectionItemValueConnections(existingEntity, operationModel.CollectionItemValueList);
                if (statuscode != 200)
                {
                    scope.Dispose();
                    return (statusmessage);
                }

                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "insert"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Save(collectionItemPicture, pictureId, false); if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "update"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Save(collectionItemPicture, pictureId, true); if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statusmessage);
                    }
                }
                foreach (var (collectionItemPicture, pictureId, process) in pictureList.Where(x => x.process == "delete"))
                {
                    (statuscode, statusmessage) = processPicturePhysically.Delete(pictureId); if (statuscode != 200)
                    {
                        scope.Dispose();
                        return (statusmessage);
                    }
                }

                scope.Complete();

                return ("Sammlerstück wurde aktualisiert.");
            }
            catch (Exception ex)
            {
                return ("Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }

            void ChangeEntity(CollectionItemOperationParameterModel operationModel, CollectionItemEntity existingEntity)
            {
                bool hasChanges = false;
                if (existingEntity.CollectionItemPotentialID != operationModel.CollectionItemEntity.CollectionItemPotentialID)
                {
                    existingEntity.CollectionItemPotentialID = operationModel.CollectionItemEntity.CollectionItemPotentialID;
                    hasChanges = true;
                }
                if (existingEntity.Comment != operationModel.CollectionItemEntity.Comment)
                {
                    existingEntity.Comment = operationModel.CollectionItemEntity.Comment;
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
                if (existingEntity.ProductionSize != operationModel.CollectionItemEntity.ProductionSize)
                {
                    existingEntity.ProductionSize = operationModel.CollectionItemEntity.ProductionSize;
                    hasChanges = true;
                }
                if (existingEntity.TransferFromOwner != operationModel.CollectionItemEntity.TransferFromOwner)
                {
                    existingEntity.TransferFromOwner = operationModel.CollectionItemEntity.TransferFromOwner;
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
                if (existingEntity.ProcessOfManufactureID != operationModel.CollectionItemEntity.ProcessOfManufactureID)
                {
                    existingEntity.ProcessOfManufactureID = operationModel.CollectionItemEntity.ProcessOfManufactureID;
                    hasChanges = true;
                }
                if (existingEntity.ConceptID != operationModel.CollectionItemEntity.ConceptID)
                {
                    existingEntity.ConceptID = operationModel.CollectionItemEntity.ConceptID;
                    hasChanges = true;
                }
                if (existingEntity.UniqueName != operationModel.CollectionItemEntity.UniqueName)
                {
                    existingEntity.UniqueName = operationModel.CollectionItemEntity.UniqueName;
                    hasChanges = true;
                }
                if (existingEntity.Inscription != operationModel.CollectionItemEntity.Inscription)
                {
                    existingEntity.Inscription = operationModel.CollectionItemEntity.Inscription;
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
            }
        }

        public string Delete(CollectionItemOperationParameterModel model)
        {
            CollectionItemOperationParameterModel? existingOperationParameterModel = GetWithPredicates(ParametersOperationToSearch(model)).FirstOrDefault();
            if (existingOperationParameterModel == null)
            {
                return ("Sammlerstück nicht gefunden");
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
                for (int i = existingOperationParameterModel.CollectionItemValueList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    (int statuscode, string statusmessage) = processCollectionItemValue.Delete(existingOperationParameterModel.CollectionItemValueList[index].CollectionItemValueID);
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

                return ("Sammlerstück wurde entfernt.");
            }
            catch (Exception ex)
            {
                return ("Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public List<CollectionItemOperationParameterModel> GetWithPredicates(CollectionItemSearchParameterModel model)
        {
            IEnumerable<CollectionItemEntity> collectionItemIEnumberable = unitOfWork.CollectionItemEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemEntity>(model),
                includeProperties: GetIncludeProperties());

            return [..from b in collectionItemIEnumberable
                  select new CollectionItemOperationParameterModel
                  {
                      CollectionItemEntity = b,
                      CollectionItemPotential = b.CollectionItemPotential ?? new(),
                      CollectionItemPictureList = b.CollectionItemPictureList,
                      ProcessOfManufacture = b.ProcessOfManufacture ?? new() { Mainprocess = string.Empty, ProcessOfManufactureName = string.Empty },
                      CollectionItemNColorList = b.CollectionItemNColorList,
                      CollectionItemNMaterialList = b.CollectionItemNMaterialList,
                      StateList = [.. unitOfWork.StateRepository.Get()],
                      ColorList = [.. unitOfWork.ColorRepository.Get()],
                      MaterialList = [.. unitOfWork.MaterialRepository.Get()],
                      CollectionAttributeList = [.. unitOfWork.CollectionAttributeRepository.Get(filter: ca => ca.CollectionAreaID == b.CollectionAreaID)],
                      CollectionItemValueList = b.CollectionItemValueList,
                      CollectionItemNPartyList = b.CollectionItemNPartyList,
                      CollectionItemNPlaceList = b.CollectionItemNPlaceList,
                      Concept = b.Concept ?? new() {ConceptName = string.Empty},
                      Era = b.Era ?? new() {EraName = string.Empty}
                  }];
        }

        

        private static string GetIncludeProperties()
        {
            return "CollectionItemPictureList," +
                   "UsingIdentityUser," +
                   "ProcessOfManufacture," +
                   "Concept," +
                   "CollectionItemNColorList.Color," +
                   "CollectionItemPotential," +
                   "CollectionItemNMaterialList.Material," +
                   "CollectionItemValueList," +
                   "CollectionItemNPlaceList.Place.PlaceNToponymyList.Toponymy," +
                   "CollectionItemNPlaceList.Place.Settlement.SettlementNPostalcodeList.Postalcode," +
                   "CollectionItemNPartyList.Party," +
                   "Era," +
                   "State";
        }

        private void ConnectPartyToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int partyID, string? relationship)
        {
            if (partyID <= 0)
            {
                return;
            }
            Party? party = unitOfWork.PartyRepository.GetByID(partyID);
            if (party is null)
            {
                return;
            }

            CollectionItemNParty collectionItemEntityNParty = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                PartyID = partyID,
                Relationship = relationship
            };
            _ = unitOfWork.CollectionItemNPartyRepository.Insert(collectionItemEntityNParty);
            unitOfWork.Save();
        }
        private void SyncPartyConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNParty> newConnections)
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

            foreach (CollectionItemNParty newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PartyID == newItem.PartyID);
                if (!exists)
                {
                    ConnectPartyToCollectionItemEntity(existingCollectionItemEntity, newItem.PartyID, newItem.Relationship);
                }
            }
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

        private void ConnectPlaceToCollectionItemEntity(CollectionItemEntity collectionItemEntity, int placeID, string? relationship)
        {
            if (placeID <= 0)
            {
                return;
            }

            Place? place = unitOfWork.PlaceRepository.GetByID(placeID);
            if (place is null)
            {
                return;
            }

            CollectionItemNPlace collectionItemEntityNPlace = new()
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                PlaceID = placeID,
                Relationship = relationship
            };
            _ = unitOfWork.CollectionItemNPlaceRepository.Insert(collectionItemEntityNPlace);
            unitOfWork.Save();
        }
        private void SyncPlaceConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNPlace> newConnections)
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

            foreach (CollectionItemNPlace newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.PlaceID);
                if (!exists)
                {
                    ConnectPlaceToCollectionItemEntity(existingCollectionItemEntity, newItem.PlaceID, newItem.Relationship);
                }
            }
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

        private void ConnectColorToCollectionItemEntity(CollectionItemEntity collectionItem, CollectionItemNColor collectionItemNColor)
        {
            if (collectionItemNColor.ColorID <= 0)
            {
                return;
            }

            Color? color = unitOfWork.ColorRepository.GetByID(collectionItemNColor.ColorID);
            if (color is null)
            {
                return;
            }

            collectionItemNColor.CollectionItemEntityID = collectionItem.CollectionItemEntityID;

            _ = unitOfWork.CollectionItemNColorRepository.Insert(collectionItemNColor);
            unitOfWork.Save();
        }
        private void SyncColorConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNColor> newConnections)
        {
            List<CollectionItemNColor> currentConnections = existingCollectionItemEntity.CollectionItemNColorList;

            foreach (CollectionItemNColor? current in currentConnections)
            {
                CollectionItemNColor? updated = newConnections.FirstOrDefault(x => x.ColorID == current.ColorID);

                if (updated == null)
                {
                    DisconnectColorConnection(existingCollectionItemEntity, current.ColorID);
                }
                else if (updated is not null && (updated.IsPrimaryColor != current.IsPrimaryColor || updated.Note != current.Note))
                {
                    UpdateCollectionItemNColor(existingCollectionItemEntity, updated);
                }
            }

            foreach (CollectionItemNColor newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ColorID == newItem.ColorID);
                if (!exists)
                {
                    ConnectColorToCollectionItemEntity(existingCollectionItemEntity, newItem);
                }
            }
        }
        private void UpdateCollectionItemNColor(CollectionItemEntity existingCollectionItemEntity, CollectionItemNColor updated)
        {
            CollectionItemNColor? collectionItemNColor = (from pnc in unitOfWork.CollectionItemNColorRepository.Get(includeProperties: "Color")
                                                                        where pnc.ColorID == updated.ColorID && pnc.CollectionItemEntity == existingCollectionItemEntity
                                                                        select pnc).FirstOrDefault();

            if (collectionItemNColor != null)
            {
                collectionItemNColor.IsPrimaryColor = updated.IsPrimaryColor;
                collectionItemNColor.Note = updated.Note;
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

        private void ConnectMaterialToCollectionItemEntity(CollectionItemEntity collectionItem, CollectionItemNMaterial collectionItemNMatreial)
        {
            if (collectionItemNMatreial.MaterialID <= 0)
            {
                return;
            }

            Material? material = unitOfWork.MaterialRepository.GetByID(collectionItemNMatreial.MaterialID);
            if (material is null)
            {
                return;
            }

            collectionItemNMatreial.CollectionItemEntityID = collectionItem.CollectionItemEntityID;

            _ = unitOfWork.CollectionItemNMaterialRepository.Insert(collectionItemNMatreial);
            unitOfWork.Save();
        }
        private void SyncMaterialConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemNMaterial> newConnections)
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

            foreach (CollectionItemNMaterial newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.MaterialID == newItem.MaterialID);
                if (!exists)
                {
                    ConnectMaterialToCollectionItemEntity(existingCollectionItemEntity, newItem);
                }
            }
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
            List<(CollectionItemPicture CollectionItemPicture, int PictureId, string Process)> pictureResults = new();

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
                    if (updated.Datei != null)
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
            return (pictureResults, 200, "Pictures synchronized successfully.");
        }

        private (int Statuscode, string Statusmessage) SyncCollectionItemValueConnections(CollectionItemEntity existingCollectionItemEntity, List<CollectionItemValue> newConnections)
        {
            List<CollectionItemValue> currentConnections = existingCollectionItemEntity.CollectionItemValueList;
            int statusCode;
            string statusMessage;

            for (int i = 0; i < currentConnections.Count; i++)
            {
                CollectionItemValue? updated = newConnections.FirstOrDefault(x => x.CollectionItemValueID == currentConnections[i].CollectionItemValueID);

                if (updated == null)
                {
                    (statusCode, statusMessage) = processCollectionItemValue.Delete(currentConnections[i].CollectionItemValueID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage);
                    }
                }
                else if (updated != null)
                {
                    (statusCode, statusMessage) = processCollectionItemValue.Update(updated);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage);
                    }
                }
            }

            foreach (CollectionItemValue newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.CollectionAttributeID == newItem.CollectionAttributeID);
                if (!exists)
                {
                    (statusCode, statusMessage) = processCollectionItemValue.Insert(newItem, existingCollectionItemEntity.CollectionItemEntityID);
                    if (statusCode != 200)
                    {
                        return (statusCode, statusMessage);
                    }
                }
            }
            return (200, "CollectionItemValues synchronized successfully.");
        }
    }
}
