using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessSettlement
    {
        (int Statuscode, string Message, int PlaceID) Insert(SettlementOperationParameterModel settlementOperationParameterModel);
        (int Statuscode, string Message, int PlaceID) Update(SettlementOperationParameterModel settlementOperationParameterModel);
        void Delete(int settlementID);
    }

    public class SettlementProcessor(IProcessPlace processPlace,
                                    IUnitOfWork unitOfWork,
                                    IProcessPostalcode processPostalcode,
                                    IProcessTranslations processTranslations,
                                    ITrackEvents trackEvents) : IProcessSettlement
    {
        public (int Statuscode, string Message, int PlaceID) Insert(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                    !settlementOperationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("SettlementProcessor.Insert: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (412, "Error_PlaceName_Missing", 0);
            }

            (bool flowControl, (int Statuscode, string Message, int PlaceID) value) = IsPlaceExistingProcessCreate(settlementOperationParameterModel);
            if (!flowControl)
            {
                return value;
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                PlaceOperationParameterModel placeOperationParameter = new()
                {
                    Place = settlementOperationParameterModel.Place,
                    PlaceNToponymyList = settlementOperationParameterModel.PlaceNToponymyList,
                    ChildPlaceList = settlementOperationParameterModel.ChildPlaceList
                };
                (int Statuscode, string Message, Place Place) newPlace = processPlace.Insert(placeOperationParameter);

                settlementOperationParameterModel.Settlement.PlaceID = newPlace.Place.PlaceID;
                Settlement newSettlement = unitOfWork.SettlementRepository.Insert(settlementOperationParameterModel.Settlement);
                unitOfWork.Save();

                foreach (SettlementNPostalcode postalcode in settlementOperationParameterModel.SettlementNPostalcodeList)
                {
                    ConnectPostalcode(newSettlement, postalcode.Postalcode.PostalcodeNumber, postalcode.IsCurrentPostalcode);
                }

                transactionScope.Complete();
                return (201, "Success_Place_Created", newSettlement.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "SettlementProcessor.Insert: Error occurred while creating Settlement.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            PlaceSearchParameterModel placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. settlementOperationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [settlementOperationParameterModel.Place.ToponymyTypeInt],
                PlaceNToponymyList_Toponymy_ToponymyID = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(Toponymy)],
                        TranslatedText = [.. settlementOperationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)]
                    }).Select(x => x.EntityId)]
            };
            if (placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID.Count == 0)
            {
                placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID = [0];
            }
            List<string>? names = settlementOperationParameterModel.Settlement.RelatedGeography?.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName).ToList();
            if (names != null && names.Count > 0)
            {
                placeSearchParameter.Settlement_RelatedGeography_PlaceNToponymyList_Toponymy_ToponymyName = names;
            }
            if (settlementOperationParameterModel.SettlementNPostalcodeList.Where(x => !string.IsNullOrWhiteSpace(x.Postalcode.PostalcodeNumber)).Select(s => s.Postalcode.PostalcodeNumber).ToList().Count > 0)
            {
                placeSearchParameter.Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber = [.. settlementOperationParameterModel.SettlementNPostalcodeList.Where(x => !string.IsNullOrWhiteSpace(x.Postalcode.PostalcodeNumber)).Select(s => s.Postalcode.PostalcodeNumber)];
            }
            Place? placeExists = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (placeExists != null)
            {
                trackEvents.TrackWarning("SettlementProcessor.IsPlaceExistingProcessCreate: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "PlaceNToponymyList", settlementOperationParameterModel.PlaceNToponymyList },
                    { "Toponymy", settlementOperationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (flowControl: false, value: (409, "Error_Place_Exists", 0));
            }

            return (flowControl: true, value: default);
        }

        public (int Statuscode, string Message, int PlaceID) Update(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("SettlementProcessor.Update: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                !settlementOperationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("SettlementProcessor.Update: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (412, "Error_PlaceName_Missing", settlementOperationParameterModel.Place.PlaceID);
            }

            PlaceSearchParameterModel placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(settlementOperationParameterModel.Place.PlaceID);
            Settlement? existingSettlement = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Settlement;
            if (existingSettlement == null)
            {
                trackEvents.TrackWarning("SettlementProcessor.Update: Settlement not found.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (404, "Error_Place_NotFound", new());
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (existingSettlement.RelatedGeographyID != settlementOperationParameterModel.Settlement.RelatedGeographyID
                    || existingSettlement.Byname != settlementOperationParameterModel.Settlement.Byname)
                {
                    existingSettlement.RelatedGeographyID = settlementOperationParameterModel.Settlement.RelatedGeographyID;
                    existingSettlement.Byname = settlementOperationParameterModel.Settlement.Byname;
                    unitOfWork.Save();
                }

                PlaceOperationParameterModel placeOperationParameterModel = new()
                {
                    Place = settlementOperationParameterModel.Place,
                    PlaceNToponymyList = settlementOperationParameterModel.PlaceNToponymyList,
                    ChildPlaceList = settlementOperationParameterModel.ChildPlaceList
                };
                _ = processPlace.Update(placeOperationParameterModel);

                SyncPostalcode(existingSettlement, settlementOperationParameterModel.SettlementNPostalcodeList);

                transactionScope.Complete();
                return (200, "Success_Place_Updated", existingSettlement.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "SettlementProcessor.Update: Error occurred while updating Settlement.", new Dictionary<string, object>
                {
                    { "Place", settlementOperationParameterModel.Place},
                    { "Settlement", settlementOperationParameterModel.Settlement}
                });
                return (500, "Error_Error_Ocurred", existingSettlement.PlaceID);
            }
        }

        private void ConnectPostalcode(Settlement settlement, string Postalcode, bool currentPostalcode)
        {
            if (string.IsNullOrWhiteSpace(Postalcode))
            {
                return;
            }

            Postalcode? postalcode = processPostalcode.CreateOrGetPostalcode(Postalcode);

            SettlementNPostalcode settlementNPostalcode = new()
            {
                SettlementID = settlement.SettlementID,
                PostalcodeID = postalcode.PostalcodeID,
                IsCurrentPostalcode = currentPostalcode
            };
            _ = unitOfWork.SettlementNPostalcodeRepository.Insert(settlementNPostalcode);
            unitOfWork.Save();
        }
        private void SyncPostalcode(Settlement settlement, List<SettlementNPostalcode> newConnections)
        {
            List<SettlementNPostalcode> currentConnections = settlement.SettlementNPostalcodeList;

            for (int i = 0; i < currentConnections.Count; i++)
            {
                SettlementNPostalcode? updatedConnection = newConnections.FirstOrDefault(c => c.Postalcode.PostalcodeNumber == currentConnections[i].Postalcode.PostalcodeNumber);
                if (updatedConnection == null)
                {
                    DisconnectPostalcode(settlement, currentConnections[i].Postalcode.PostalcodeID);
                }
                else if (updatedConnection.IsCurrentPostalcode != currentConnections[i].IsCurrentPostalcode)
                {
                    UpdateSettlementNPostalcode(settlement, currentConnections[i].Postalcode, updatedConnection.IsCurrentPostalcode);
                }
            }

            foreach (SettlementNPostalcode newItem in newConnections)
            {
                bool exists = currentConnections.Any(c => c.Postalcode.PostalcodeNumber == newItem.Postalcode.PostalcodeNumber);
                if (!exists)
                {
                    ConnectPostalcode(settlement, newItem.Postalcode.PostalcodeNumber, newItem.IsCurrentPostalcode);
                }
            }
        }
        private void UpdateSettlementNPostalcode(Settlement Settlement, Postalcode postalcode, bool currentPostalcode)
        {
            SettlementNPostalcode? existingSettlementNPostalcode = unitOfWork.SettlementNPostalcodeRepository.Get(
                filter: c => c.SettlementID == Settlement.SettlementID && c.PostalcodeID == postalcode.PostalcodeID).FirstOrDefault();

            if (existingSettlementNPostalcode != null)
            {
                existingSettlementNPostalcode.IsCurrentPostalcode = currentPostalcode;
                unitOfWork.Save();
            }
        }
        private void DisconnectPostalcode(Settlement settlement, int postalcodeID)
        {
            if (settlement.SettlementID == 0 || postalcodeID == 0)
            {
                return;
            }

            SettlementNPostalcode? settlementNPostalcode = unitOfWork.SettlementNPostalcodeRepository.Get(
                filter: c => c.SettlementID == settlement.SettlementID && c.PostalcodeID == postalcodeID).FirstOrDefault();
            if (settlementNPostalcode != null)
            {
                unitOfWork.SettlementNPostalcodeRepository.Delete(settlementNPostalcode);
                unitOfWork.Save();
            }
        }

        public void Delete(int settlementID)
        {
            throw new NotImplementedException();
        }
    }
}
