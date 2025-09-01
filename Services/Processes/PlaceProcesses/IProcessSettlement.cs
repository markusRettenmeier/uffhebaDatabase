using Sammlerplattform.Data;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessSettlement
    {
        (int PlaceID, int Statuscode, string Message) CreateSettlement(SettlementOperationParameterModel settlementOperationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditSettlement(SettlementOperationParameterModel settlementOperationParameterModel);
        void DeleteSettlement(int settlementID);
    }

    public class SettlementProcessor(IProcessPlace processPlace,
                                        IUnitOfWork unitOfWork,
                                        IProcessPostalcode processPostalcode) : IProcessSettlement
    {
        public (int PlaceID, int Statuscode, string Message) CreateSettlement(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                    !settlementOperationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (0, 412, "Ortsnamen angeben.");
            }

            PlaceSearchParameter placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. settlementOperationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [settlementOperationParameterModel.Place.ToponymyTypeInt],
                Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber = [.. settlementOperationParameterModel.SettlementNPostalcodeList.Where(x => !string.IsNullOrWhiteSpace(x.Postalcode.PostalcodeNumber)).Select(s => s.Postalcode.PostalcodeNumber)]
            };
            Place? placeExists = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (placeExists != null)
            {
                return (placeExists.PlaceID, 409, "Ort existiert bereits.");
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
                var newPlace = processPlace.CreatePlace(placeOperationParameter);

                settlementOperationParameterModel.Settlement.PlaceID = newPlace.Place.PlaceID;
                Settlement newSettlement = unitOfWork.SettlementRepository.Insert(settlementOperationParameterModel.Settlement);
                unitOfWork.Save();

                foreach(var postalcode in settlementOperationParameterModel.SettlementNPostalcodeList)
                {
                    ConnectPostalcode(newSettlement, postalcode.Postalcode.PostalcodeNumber, postalcode.IsCurrentPostalcode);
                }

                transactionScope.Complete();
                return (newSettlement.PlaceID, 201, "Siedlung erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Siedlung: {ex}", ex);
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
        public (int PlaceID, int Statuscode, string Message) EditSettlement(SettlementOperationParameterModel settlementOperationParameterModel)
        {
            if (settlementOperationParameterModel.Place.PlaceID == 0)
            {
                return (new(), 412, "Orts-ID ist leer.");
            }
            if (settlementOperationParameterModel.PlaceNToponymyList == null ||
                !settlementOperationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (settlementOperationParameterModel.Place.PlaceID, 412, "Ortsnamen angeben.");
            }

            PlaceSearchParameter placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(settlementOperationParameterModel.Place.PlaceID);
            Settlement? existingSettlement = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Settlement;
            if (existingSettlement == null)
            {
                return (new(), 404, "Siedlung nicht gefunden.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (existingSettlement.RelatedPlaceID != settlementOperationParameterModel.Settlement.RelatedPlaceID
                    || existingSettlement.Byname != settlementOperationParameterModel.Settlement.Byname)
                {
                    existingSettlement.RelatedPlaceID = settlementOperationParameterModel.Settlement.RelatedPlaceID;
                    existingSettlement.Byname = settlementOperationParameterModel.Settlement.Byname;
                    unitOfWork.Save();
                }

                PlaceOperationParameterModel placeOperationParameterModel = new()
                {
                    Place = settlementOperationParameterModel.Place,                    
                    PlaceNToponymyList = settlementOperationParameterModel.PlaceNToponymyList,
                    ChildPlaceList = settlementOperationParameterModel.ChildPlaceList
                };
                processPlace.EditPlace(placeOperationParameterModel);

                SyncPostalcode(existingSettlement, settlementOperationParameterModel.SettlementNPostalcodeList);

                transactionScope.Complete();
                return (existingSettlement.PlaceID, 200, "Siedlung erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Siedlung: {ex}", ex);
                return (existingSettlement.PlaceID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        private void ConnectPostalcode(Settlement settlement, string Postalcode/*, int? eraID*/, bool currentPostalcode)
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
                else if (/*updatedConnection.EraID != currentConnections[i].EraID||*/ updatedConnection.IsCurrentPostalcode != currentConnections[i].IsCurrentPostalcode)
                {
                    UpdateSettlementNPostalcode(settlement, currentConnections[i].Postalcode/*, updatedConnection.EraID*/, updatedConnection.IsCurrentPostalcode);
                }
            }

            foreach (SettlementNPostalcode newItem in newConnections)
            {
                bool exists = currentConnections.Any(c => c.Postalcode.PostalcodeNumber == newItem.Postalcode.PostalcodeNumber);
                if (!exists)
                {
                    ConnectPostalcode(settlement, newItem.Postalcode.PostalcodeNumber/*, newItem.EraID*/, newItem.IsCurrentPostalcode);
                }
            }
        }
        private void UpdateSettlementNPostalcode(Settlement Settlement, Postalcode postalcode/*, int? eraID*/, bool currentPostalcode)
        {
            SettlementNPostalcode? existingSettlementNPostalcode = unitOfWork.SettlementNPostalcodeRepository.Get(
                filter: c => c.SettlementID == Settlement.SettlementID && c.PostalcodeID == postalcode.PostalcodeID).FirstOrDefault();

            if (existingSettlementNPostalcode != null)
            {
                //existingSettlementNPostalcode.EraID = eraID;
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
