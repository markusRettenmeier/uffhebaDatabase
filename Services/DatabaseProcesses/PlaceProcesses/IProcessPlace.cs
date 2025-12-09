using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessPlace
    {
        List<Place> GetListWithPredicate(PlaceSearchParameter placeSearchParameter);
        (Place Place, int Statuscode, string Message) Create(PlaceOperationParameterModel operationParameter);
        (Place Place, int Statuscode, string Message) Edit(PlaceOperationParameterModel operationParameter);
        (Place Place, int Statuscode, string Message) Delete(int placeID);
    }

    public class PlaceProcessor(IUnitOfWork unitOfWork,
        IProcessToponymy processToponymy
        , DeeplTranslationService translationService
        , IProcessTranslations processTranslations
        , ITranslationStore translationStore) : IProcessPlace
    {
        public (Place Place, int Statuscode, string Message) Create(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null ||
                    !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (new(), 412, "Error_PlaceName_Missing");
            }

            if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
            {
                return (new(), 412, "Error_ParentPlace_ParentOfItsOwn");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Place newPlace = unitOfWork.PlaceRepository.Insert(operationParameter.Place);
                unitOfWork.Save();

                foreach (PlaceNToponymy placeNToponymy in operationParameter.PlaceNToponymyList)
                {
                    ConnectToponymy(newPlace, placeNToponymy);
                }
                foreach (Place childPlace in operationParameter.ChildPlaceList)
                {
                    newPlace.ChildPlaceList.Add(childPlace);
                    unitOfWork.Save();
                }

                scope.Complete();
                return (newPlace, 201, "Success_Place_Created");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Error_Error_Ocurred");
            }
        }

        public (Place Place, int Statuscode, string Message) Delete(int placeID)
        {
            if (placeID <= 0)
            {
                return (new(), 400, "Error_PlaceID_Missing");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Place? placeToDelete = unitOfWork.PlaceRepository.GetByID(placeID);
                if (placeToDelete == null)
                {
                    return (new(), 404, "Error_Place_NotFound");
                }

                unitOfWork.PlaceRepository.Delete(placeToDelete);
                unitOfWork.Save();

                scope.Complete();
                return (placeToDelete, 200, "Success_Place_Deleted");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Löschen des Ortes: {ex}", ex);
                return (new(), 500, "Error_Error_Ocurred");
            }
        }

        public (Place Place, int Statuscode, string Message) Edit(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null || !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (new(), 412, "Error_PlaceName_Missing");
            }

            PlaceSearchParameter placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameter.Place.PlaceID);
            Place? existingPlace = GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                return (new(), 404, "Error_Place_NotFound");
            }

            if (existingPlace.ParentPlaceID != operationParameter.Place.ParentPlaceID && operationParameter.Place.ParentPlaceID > 0)
            {
                if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
                {
                    return (new(), 412, "Error_ParentPlace_ParentOfItsOwn");
                }
                existingPlace.ParentPlaceID = operationParameter.Place.ParentPlaceID;
                unitOfWork.Save();
            }
            SyncChildPlaces(existingPlace, operationParameter.ChildPlaceList);
            SyncToponymy(existingPlace, operationParameter.PlaceNToponymyList);

            return (existingPlace, 200, "Success_Party_Updated");
        }
        public List<Place> GetListWithPredicate(PlaceSearchParameter placeSearchParameter)
        {
            IEnumerable<Place> placeIEnumerable = unitOfWork.PlaceRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Place>(placeSearchParameter),
                includeProperties: "PlaceNToponymyList.Toponymy," +
                "ParentPlace.PlaceNToponymyList.Toponymy," +
                "ParentPlace.Settlement.SettlementNPostalcodeList.Postalcode," +
                "ChildPlaceList.PlaceNToponymyList.Toponymy," +
                "ChildPlaceList.Settlement.SettlementNPostalcodeList.Postalcode," +
                "Settlement.SettlementNPostalcodeList.Postalcode," +
                "Settlement.RelatedGeography.PlaceNToponymyList.Toponymy," +
                "RelatedSettlement," +
                "BodyOfWater," +
                "Building," +
                "Field," +
                "Region," +
                "Relief," +
                "TransportRoute");

            List<Place> placeList = [.. placeIEnumerable];
            foreach (Place place in placeList)
            {
                foreach (PlaceNToponymy placeNToponymy in place.PlaceNToponymyList)
                {
                    placeNToponymy.Toponymy.ToponymyName = translationStore.GetTranslation(
                        nameof(Toponymy),
                        placeNToponymy.Toponymy.ToponymyID,
                        nameof(Toponymy.ToponymyName),
                        placeNToponymy.Toponymy.ToponymyName)
                        ?? placeNToponymy.Toponymy.ToponymyName;
                }
            }

            return [.. placeList.OrderBy(p => p.PlaceNToponymyList
                .Where(t => t.IsCurrentName)
                .Select(t => t.Toponymy.ToponymyName)
                .FirstOrDefault())];
        }

        private void ConnectToponymy(Place place, PlaceNToponymy placeNToponymy)
        {
            if (string.IsNullOrWhiteSpace(placeNToponymy.Toponymy.ToponymyName))
            {
                return;
            }

            string toponymyEnglish = translationService.SetIntoFallbackLanguage(placeNToponymy.Toponymy.ToponymyName);

            Toponymy newToponymy = new() { ToponymyName = toponymyEnglish };
            newToponymy = processToponymy.CreateOrEditToponymy(newToponymy);

            PlaceNToponymy newPlaceNToponymy = new()
            {
                PlaceID = place.PlaceID,
                ToponymyID = newToponymy.ToponymyID,
                IsCurrentName = placeNToponymy.IsCurrentName
            };
            _ = unitOfWork.PlaceNToponomyRepository.Insert(newPlaceNToponymy);
            unitOfWork.Save();

            processTranslations.Create(
                new EntityTranslation
                {
                    EntityType = nameof(Toponymy),
                    EntityId = newToponymy.ToponymyID,
                    FieldName = nameof(Toponymy.ToponymyName),
                    TranslatedText = placeNToponymy.Toponymy.ToponymyName,
                    Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                },
                placeNToponymy.Toponymy.ToponymyName);
        }
        private void SyncToponymy(Place place, List<PlaceNToponymy> newConnections)
        {
            List<PlaceNToponymy> currentConnections = place.PlaceNToponymyList;

            for (int i = 0; i < currentConnections.Count; i++)
            {
                PlaceNToponymy? updatedConnection = newConnections.FirstOrDefault(x => x.Toponymy != null && x.Toponymy.ToponymyName == currentConnections[i].Toponymy.ToponymyName);
                if (updatedConnection == null)
                {
                    DisconnectToponymy(place, currentConnections[i].ToponymyID);
                }
                else if (updatedConnection != null && (
                    updatedConnection.IsCurrentName != currentConnections[i].IsCurrentName))
                {
                    //Wenn sich Ortsname ändert, dann soll Eintrag gelöscht werden
                    UpdatePlaceNToponymy(place, currentConnections[i], updatedConnection.IsCurrentName);
                }
                // else: Beziehung ist gleich, keine Änderung notwendig
            }

            foreach (PlaceNToponymy newItem in newConnections.Where(x => x.Toponymy != null))
            {
                bool exists = currentConnections.Any(x => x.Toponymy.ToponymyName == newItem.Toponymy.ToponymyName);
                if (!exists)
                {
                    ConnectToponymy(place, newItem);
                }
            }
        }
        private void UpdatePlaceNToponymy(Place place, PlaceNToponymy placeNToponymy, bool currentName)
        {
            PlaceNToponymy? existingPlaceNToponymy = unitOfWork.PlaceNToponomyRepository.Get(
                filter: c => c.PlaceID == place.PlaceID && c.ToponymyID == placeNToponymy.ToponymyID).FirstOrDefault();
            if (existingPlaceNToponymy != null)
            {
                existingPlaceNToponymy.IsCurrentName = currentName;
                unitOfWork.Save();
            }
        }
        private void DisconnectToponymy(Place place, int toponymyID)
        {
            if (place.PlaceID == 0 || toponymyID == 0)
            {
                return;
            }

            PlaceNToponymy? placeNToponymy = unitOfWork.PlaceNToponomyRepository.Get(
                filter: c => c.PlaceID == place.PlaceID && c.ToponymyID == toponymyID).FirstOrDefault();
            if (placeNToponymy == null)
            {
                return;
            }
            unitOfWork.PlaceNToponomyRepository.Delete(placeNToponymy);
            unitOfWork.Save();
        }

        private void SyncChildPlaces(Place place, List<Place> newConnections)
        {
            List<Place> currentConnections = place.ChildPlaceList;

            for (int i = 0; i < currentConnections.Count; i++)
            {
                Place? updatedConnection = newConnections.FirstOrDefault(x => x.PlaceID == currentConnections[i].PlaceID);
                if (updatedConnection == null)
                {
                    unitOfWork.PlaceRepository.RemoveMemberFromCollection(place, p => p.ChildPlaceList, currentConnections[i]);
                    unitOfWork.Save();
                }
            }

            foreach (Place newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.PlaceID);
                if (!exists)
                {
                    if (newItem.PlaceID == place.PlaceID)
                    {
                        continue; // Ein Ort kann nicht Elternteil von sich selbst sein.
                    }
                    unitOfWork.PlaceRepository.AddMemberToCollection(place, p => p.ChildPlaceList, newItem);
                    unitOfWork.Save();
                }
            }
        }
    }
}
