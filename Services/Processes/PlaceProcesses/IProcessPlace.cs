using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessPlace
    {
        List<Place> GetListWithPredicate(PlaceSearchParameter placeSearchParameter);
        (Place Place, int Statuscode, string Message) Create(PlaceOperationParameterModel operationParameter);
        (Place Place, int Statuscode, string Message) Edit(PlaceOperationParameterModel operationParameter);
        (Place Place, int Statuscode, string Message) Delete(int placeID);
    }

    public class PlaceProcessor(IUnitOfWork unitOfWork,
        IProcessToponymy processToponymy) : IProcessPlace
    {
        public (Place Place, int Statuscode, string Message) Create(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null ||
                    !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (new(), 412, "Geografischer Name angeben.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
                {
                    return (new(), 412, "Ein Ort kann nicht Elternteil von sich selbst sein.");
                }
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
                return (newPlace, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (Place Place, int Statuscode, string Message) Delete(int placeID)
        {
            if (placeID <= 0)
            {
                return (new(), 400, "Ungültige Orts-ID.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Place? placeToDelete = unitOfWork.PlaceRepository.GetByID(placeID);
                if (placeToDelete == null)
                {
                    return (new(), 404, "Ort nicht gefunden.");
                }

                unitOfWork.PlaceRepository.Delete(placeToDelete);
                unitOfWork.Save();

                scope.Complete();
                return (placeToDelete, 200, "Ort erfolgreich gelöscht.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Löschen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (Place Place, int Statuscode, string Message) Edit(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null || !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (new(), 412, "Geografischer Name angeben.");
            }

            PlaceSearchParameter placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameter.Place.PlaceID);
            Place? existingPlace = GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                return (new(), 404, "Ort nicht gefunden.");
            }

            if (existingPlace.ParentPlaceID != operationParameter.Place.ParentPlaceID && operationParameter.Place.ParentPlaceID > 0)
            {
                if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
                {
                    return (new(), 412, "Ein Ort kann nicht Elternteil von sich selbst sein.");
                }
                existingPlace.ParentPlaceID = operationParameter.Place.ParentPlaceID;
                unitOfWork.Save();
            }
            SyncChildPlaces(existingPlace, operationParameter.ChildPlaceList);
            SyncToponymy(existingPlace, operationParameter.PlaceNToponymyList);

            return (existingPlace, 200, "Ort erfolgreich aktualisiert.");
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
                "Settlement.RelatedPlace.PlaceNToponymyList.Toponymy," +
                "RelatedSettlement," +
                "BodyOfWater," +
                "Building," +
                "Field," +
                "Region," +
                "Relief," +
                "TransportRoute");

            return [.. placeIEnumerable];
        }

        private void ConnectToponymy(Place place, PlaceNToponymy placeNToponymy)
        {
            if (string.IsNullOrWhiteSpace(placeNToponymy.Toponymy.ToponymyName))
            {
                return;
            }

            Toponymy newToponymy = new() { ToponymyName = placeNToponymy.Toponymy.ToponymyName };
            newToponymy = processToponymy.CreateOrEditToponymy(newToponymy);

            PlaceNToponymy newPlaceNToponymy = new()
            {
                PlaceID = place.PlaceID,
                ToponymyID = newToponymy.ToponymyID,
                IsCurrentName = placeNToponymy.IsCurrentName
            };
            _ = unitOfWork.PlaceNToponomyRepository.Insert(newPlaceNToponymy);
            unitOfWork.Save();
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
