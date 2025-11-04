using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PartyProcesses
{
    public interface IProcessParty
    {
        List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel);
        (Party Party, int Statuscode, string Message) CreateParty(PartyOperationParameterModel partyOperationParameterModel);
        (Party Party, int Statuscode, string Message) EditParty(PartyOperationParameterModel partyOperationParameterModel);
        (int Statuscode, string Message) DeleteParty(int partyID);
    }

    public class PartyProcessor(IUnitOfWork unitOfWork, IProcessPlace processPlace) : IProcessParty
    {
        public (Party Party, int Statuscode, string Message) CreateParty(PartyOperationParameterModel partyOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(partyOperationParameterModel.Party.PartyName))
            {
                return (new Party() { PartyName = string.Empty }, 412, "Parteiname angeben.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Party newParty = unitOfWork.PartyRepository.Insert(partyOperationParameterModel.Party);
                unitOfWork.Save();

                foreach (Place place in partyOperationParameterModel.PlaceList)
                {
                    ConnectPlaceToParty(newParty, place.PlaceID);
                }

                scope.Complete();
                return (newParty, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new() { PartyName = string.Empty }, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (int Statuscode, string Message) DeleteParty(int partyID)
        {
            throw new NotImplementedException();
        }

        public (Party Party, int Statuscode, string Message) EditParty(PartyOperationParameterModel partyOperationParameterModel)
        {
            if (partyOperationParameterModel.Party == null || partyOperationParameterModel.Party.PartyID <= 0)
            {
                return (new Party() { PartyName = string.Empty }, 412, "ParteiID fehlt.");
            }
            if (string.IsNullOrWhiteSpace(partyOperationParameterModel.Party.PartyName))
            {
                return (new Party() { PartyName = string.Empty }, 412, "Parteiname angeben.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Party? partyToEdit = GetListWithPredicate(
                    new PartySearchParameterModel { PartyID = [partyOperationParameterModel.Party.PartyID] }
                    ).FirstOrDefault();
                if (partyToEdit == null)
                {
                    return (new Party() { PartyName = string.Empty }, 404, "Partei nicht gefunden.");
                }

                if (partyToEdit.PartyName != partyOperationParameterModel.Party.PartyName
                    || partyToEdit.PartyDescription != partyOperationParameterModel.Party.PartyDescription)
                {
                    partyToEdit.PartyName = partyOperationParameterModel.Party.PartyName;
                    partyToEdit.PartyDescription = partyOperationParameterModel.Party.PartyDescription;
                    unitOfWork.Save();
                }

                SyncPlace(partyToEdit, partyOperationParameterModel.PlaceList);

                scope.Complete();
                return (partyToEdit, 200, "Partei erfolgreich bearbeitet.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Bearbeiten der Partei: {ex}", ex);
                return (new() { PartyName = string.Empty }, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel)
        {
            IEnumerable<Party> partyIEnumerable = unitOfWork.PartyRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Party>(partySearchParameterModel),
                includeProperties: "Individual," +
                "Organization.ProductionFacility," +
                "PlaceList.PlaceNToponymyList.Toponymy," +
                "PlaceList.Settlement.SettlementNPostalcodeList.Postalcode");

            return [.. partyIEnumerable.OrderBy(x => x.PartyName)];
        }

        private void ConnectPlaceToParty(Party party, int placeID)
        {
            if (placeID <= 0)
            {
                return;
            }

            Place? place = processPlace.GetListWithPredicate(
                new PlaceSearchParameter { PlaceID = [placeID] }
                ).FirstOrDefault();
            if (place == null)
            {
                return;
            }

            unitOfWork.PartyRepository.AddMemberToCollection(party, p => p.PlaceList, place);
            unitOfWork.Save();
        }
        private void SyncPlace(Party party, List<Place> newPlaceList)
        {
            List<Place> placesToRemove = [.. party.PlaceList.Where(p => !newPlaceList.Any(np => np.PlaceID == p.PlaceID))];
            foreach (Place? place in placesToRemove)
            {
                DisconnectPlaceFromParty(party, place.PlaceID);
            }
            List<Place> placesToAdd = [.. newPlaceList.Where(np => !party.PlaceList.Any(p => p.PlaceID == np.PlaceID))];
            foreach (Place? place in placesToAdd)
            {
                ConnectPlaceToParty(party, place.PlaceID);
            }
        }
        private void DisconnectPlaceFromParty(Party party, int placeID)
        {
            if (placeID <= 0)
            {
                return;
            }
            Place? place = processPlace.GetListWithPredicate(
                new PlaceSearchParameter { PlaceID = [placeID] }
                ).FirstOrDefault();
            if (place == null)
            {
                return;
            }
            unitOfWork.PartyRepository.RemoveMemberFromCollection(party, p => p.PlaceList, place);
            unitOfWork.Save();
        }
    }
}
