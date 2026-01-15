using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessParty
    {
        List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel);
        (int Statuscode, string Message, Party Party) Insert(PartyOperationParameterModel partyOperationParameterModel);
        (int Statuscode, string Message, Party Party) Update(PartyOperationParameterModel partyOperationParameterModel);
        (int Statuscode, string Message) Delete(int partyID);
    }

    public class PartyProcessor(IUnitOfWork unitOfWork
        , IProcessPlace processPlace
        , ITrackEvents trackEvents) : IProcessParty
    {
        public (int Statuscode, string Message, Party Party) Insert(PartyOperationParameterModel partyOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(partyOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("PartyProcessor.CreateParty: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", partyOperationParameterModel.Party },
                    { "PlaceList", partyOperationParameterModel.PlaceList }
                });
                return (412, "Error_Party_NameMissing", new Party() { PartyName = string.Empty });
            }

            Party newParty = unitOfWork.PartyRepository.Insert(partyOperationParameterModel.Party);
            unitOfWork.Save();

            foreach (Place place in partyOperationParameterModel.PlaceList)
            {
                ConnectPlaceToParty(newParty, place.PlaceID);
            }

            return (201, "Success_Party_Created", newParty);
        }

        public (int Statuscode, string Message) Delete(int partyID)
        {
            trackEvents.TrackWarning("PartyProcessor.DeleteParty: Not implemented yet.", new Dictionary<string, object>
            {
                { "PartyID", partyID }
            });
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, Party Party) Update(PartyOperationParameterModel partyOperationParameterModel)
        {
            if (partyOperationParameterModel.Party == null || partyOperationParameterModel.Party.PartyID <= 0)
            {
                trackEvents.TrackWarning("PartyProcessor.EditParty: PartyID is missing or invalid.", new Dictionary<string, object>
                {
                    { "PlaceList", partyOperationParameterModel.PlaceList }
                });
                return (412, "Error_PartyID_Missing", new Party() { PartyName = string.Empty });
            }
            if (string.IsNullOrWhiteSpace(partyOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("PartyProcessor.EditParty: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", partyOperationParameterModel.Party },
                    { "PlaceList", partyOperationParameterModel.PlaceList }
                });
                return (412, "Error_Party_NameMissing", new Party() { PartyName = string.Empty });
            }

            Party? existingParty = GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [partyOperationParameterModel.Party.PartyID] }
                ).FirstOrDefault();
            if (existingParty == null)
            {
                return (404, "Error_Party_NotFound", new Party() { PartyName = string.Empty });
            }

            bool isChanged = false;
            if (existingParty.PartyName != partyOperationParameterModel.Party.PartyName)
            {
                existingParty.PartyName = partyOperationParameterModel.Party.PartyName;
                isChanged = true;
            }
            if (existingParty.WikipediaUrl != partyOperationParameterModel.Party.WikipediaUrl)
            {
                existingParty.WikipediaUrl = partyOperationParameterModel.Party.WikipediaUrl;
                isChanged = true;
            }
            if (existingParty.PartyTypeInt != partyOperationParameterModel.Party.PartyTypeInt)
            {
                existingParty.PartyTypeInt = partyOperationParameterModel.Party.PartyTypeInt;
                isChanged = true;
            }
            if (isChanged)
            {
                unitOfWork.Save();
            }

            SyncPlace(existingParty, partyOperationParameterModel.PlaceList);

            return (200, "Success_Party_Updated", existingParty);
        }

        public List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel)
        {
            IEnumerable<Party> partyIEnumerable = unitOfWork.PartyRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Party>(partySearchParameterModel),
                includeProperties: nameof(Party.Individual) + "," +
                                nameof(Party.Organization.ProductionFacility) + "," +
                                nameof(Party.PlaceList) + "." + nameof(Place.PlaceNToponymyList) + "." + nameof(PlaceNToponymy.Toponymy) + "," +
                                nameof(Party.PlaceList) + "." + nameof(Place.Settlement.SettlementNPostalcodeList) + "." + nameof(SettlementNPostalcode.Postalcode));

            return [.. partyIEnumerable.OrderBy(x => x.PartyName)];
        }

        private void ConnectPlaceToParty(Party party, int placeID)
        {
            if (placeID <= 0)
            {
                return;
            }

            Place? place = processPlace.GetListWithPredicate(
                new PlaceSearchParameterModel { PlaceID = [placeID] }
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
                new PlaceSearchParameterModel { PlaceID = [placeID] }
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
