using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessSettlement
    {
        (int PlaceID, int Statuscode, string Message) Insert(SettlementOperationParameterModel settlementOperationParameterModel);
        (int PlaceID, int Statuscode, string Message) Update(SettlementOperationParameterModel settlementOperationParameterModel);
        void DeleteSettlement(int settlementID);
    }

    public class SettlementProcessor(IProcessPlace processPlace,
                                    IUnitOfWork unitOfWork,
                                    IProcessPostalcode processPostalcode,
                                    IProcessTranslations processTranslations) : IProcessSettlement
    {
        public (int PlaceID, int Statuscode, string Message) Insert(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                    !settlementOperationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (0, 412, "Error_PlaceName_Missing");
            }

            (bool flowControl, (int PlaceID, int Statuscode, string Message) value) = IsPlaceExistingProcessCreate(settlementOperationParameterModel);
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
                (Place Place, int Statuscode, string Message) newPlace = processPlace.Create(placeOperationParameter);

                settlementOperationParameterModel.Settlement.PlaceID = newPlace.Place.PlaceID;
                Settlement newSettlement = unitOfWork.SettlementRepository.Insert(settlementOperationParameterModel.Settlement);
                unitOfWork.Save();

                foreach (SettlementNPostalcode postalcode in settlementOperationParameterModel.SettlementNPostalcodeList)
                {
                    ConnectPostalcode(newSettlement, postalcode.Postalcode.PostalcodeNumber, postalcode.IsCurrentPostalcode);
                }

                transactionScope.Complete();
                return (newSettlement.PlaceID, 201, "Success_Place_Created");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Siedlung: {ex}", ex);
                return (0, 500, "Error_Error_Ocurred");
            }
        }

        private (bool flowControl, (int PlaceID, int Statuscode, string Message) value) IsPlaceExistingProcessCreate(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            PlaceSearchParameter placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. settlementOperationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [settlementOperationParameterModel.Place.ToponymyTypeInt],
            };

            List<int> entityIdList = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
            {
                EntityType = [nameof(Toponymy)],
                TranslatedText = [.. settlementOperationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)]
            }).Select(x => x.EntityId)];
            if (entityIdList.Count > 0)
            {
                placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID = entityIdList;
            }
            if(settlementOperationParameterModel.Settlement.RelatedGeography?.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)!= null && settlementOperationParameterModel.Settlement.RelatedGeography?.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName).ToList().Count > 0)
            {
                placeSearchParameter.Settlement_RelatedGeography_PlaceNToponymyList_Toponymy_ToponymyName = [.. settlementOperationParameterModel.Settlement.RelatedGeography?.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)];
            }
            if (settlementOperationParameterModel.SettlementNPostalcodeList.Where(x => !string.IsNullOrWhiteSpace(x.Postalcode.PostalcodeNumber)).Select(s => s.Postalcode.PostalcodeNumber).ToList().Count > 0)
            {
                placeSearchParameter.Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber = [.. settlementOperationParameterModel.SettlementNPostalcodeList.Where(x => !string.IsNullOrWhiteSpace(x.Postalcode.PostalcodeNumber)).Select(s => s.Postalcode.PostalcodeNumber)];
            }
            Place? placeExists = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (placeExists != null)
            {
                return (flowControl: false, value: (placeExists.PlaceID, 409, "Error_Place_Exists"));
            }

            return (flowControl: true, value: default);
        }

        public (int PlaceID, int Statuscode, string Message) Update(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.Place.PlaceID == 0)
            {
                return (new(), 412, "Error_PlaceID_Missing");
            }
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                !settlementOperationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (settlementOperationParameterModel.Place.PlaceID, 412, "Error_PlaceName_Missing");
            }

            PlaceSearchParameter placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(settlementOperationParameterModel.Place.PlaceID);
            Settlement? existingSettlement = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Settlement;
            if (existingSettlement == null)
            {
                return (new(), 404, "Error_Place_NotFound");
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
                _ = processPlace.Edit(placeOperationParameterModel);

                SyncPostalcode(existingSettlement, settlementOperationParameterModel.SettlementNPostalcodeList);

                transactionScope.Complete();
                return (existingSettlement.PlaceID, 200, "Success_Place_Updated");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Siedlung: {ex}", ex);
                return (existingSettlement.PlaceID, 500, "Error_Error_Ocurred");
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
                //EraID = eraID,
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

        public void DeleteSettlement(int settlementID)
        {
            throw new NotImplementedException();
        }
    }
}
