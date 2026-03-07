using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessParty
    {
        List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel);
        (int Statuscode, string Message, Party Party) Insert(PartyOperationParameterModel partyOperationParameterModel);
        (int Statuscode, string Message, Party Party) Update(PartyOperationParameterModel partyOperationParameterModel);
        (int Statuscode, string Message) Delete(Party party);
    }

    public class PartyProcessor(IUnitOfWork unitOfWork
        , IProcessPlace processPlace
        , ITranslationStore translationStore
        , IDeeplTranslationService deeplTranslationService) : IProcessParty
    {
        public (int Statuscode, string Message, Party Party) Insert(PartyOperationParameterModel partyOperationParameterModel)
        {
            Party newParty = unitOfWork.PartyRepository.Insert(partyOperationParameterModel.Party);
            unitOfWork.Save();

            //foreach (int placeID in partyOperationParameterModel.ConnectedPlaceIDList)
            //{
            //    ConnectPlaceToParty(newParty, placeID);
            //}

            return (201, "Success_Party_Created", newParty);
        }

        public (int Statuscode, string Message) Delete(Party party)
        {
            if (party.CollectionItemNPartyList != null && party.CollectionItemNPartyList.Count > 0)
            {
                return (400, "Error_Individual_ConnectedToCollectionItems");
            }

            unitOfWork.PartyRepository.Delete(party);
            unitOfWork.Save();

            return (200, "Success_Party_Deleted");
        }

        public (int Statuscode, string Message, Party Party) Update(PartyOperationParameterModel partyOperationParameterModel)
        {
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

            //SyncPlace(existingParty, partyOperationParameterModel.ConnectedPlaceIDList);

            return (200, "Success_Party_Updated", existingParty);
        }

        public List<Party> GetListWithPredicate(PartySearchParameterModel partySearchParameterModel)
        {
            IEnumerable<Party> partyIEnumerable = unitOfWork.PartyRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Party>(partySearchParameterModel),
                includeProperties: $"{nameof(Party.Individual)}," +
                                    $"{nameof(Party.Organization)}.{nameof(Organization.Industry)}");
                                   //$"{nameof(Party.Organization)}.{nameof(Organization.Industry)}," +
                                   //$"{nameof(Party.PlaceList)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}");

            List <Party> partyList = [.. partyIEnumerable];

            return [.. partyIEnumerable.OrderBy(x => x.PartyName)];
        }

        //private void ConnectPlaceToParty(Party party, int placeID)
        //{
        //    Place? place = processPlace.GetListWithPredicate(
        //        new PlaceSearchParameterModel { PlaceID = [placeID] }
        //        ).FirstOrDefault();
        //    if (place == null)
        //    {
        //        return;
        //    }

        //    unitOfWork.PartyRepository.AddMemberToCollection(party, p => p.PlaceList, place);
        //    unitOfWork.Save();
        //}
        //private void SyncPlace(Party party, List<int> newPlaceIDList)
        //{
        //    //List<Place> placesToRemove = [.. party.PlaceList.Where(p => !newPlaceIDList.Any(np => np.PlaceID == p.PlaceID))];
        //    List<Place> placesToRemove = [.. party.PlaceList.Where(p => !newPlaceIDList.Contains(p.PlaceID) )];
        //    foreach (Place? place in placesToRemove)
        //    {
        //        DisconnectPlaceFromParty(party, place.PlaceID);
        //    }
        //    //List<Place> placesToAdd = [.. newPlaceIDList.Where(np => !party.PlaceList.Any(p => p.PlaceID == np.PlaceID))];
        //    List<int> placesToAdd = [.. newPlaceIDList.Where(np => !party.PlaceList.Any(p => p.PlaceID == np))];
        //    foreach (int id in placesToAdd)
        //    {
        //        ConnectPlaceToParty(party, id);
        //    }
        //}
        //private void DisconnectPlaceFromParty(Party party, int placeID)
        //{
        //    Place? place = processPlace.GetListWithPredicate(
        //        new PlaceSearchParameterModel { PlaceID = [placeID] }
        //        ).FirstOrDefault();
        //    if (place == null)
        //    {
        //        return;
        //    }
        //    unitOfWork.PartyRepository.RemoveMemberFromCollection(party, p => p.PlaceList, place);
        //    unitOfWork.Save();
        //}
    }
}
