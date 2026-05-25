using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Services.Extensions;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessPlace
    {
        List<Place> GetListWithPredicate(PlaceSearchParameterModel placeSearchParameter);
        (int Statuscode, string Message, int PlaceID) Insert(PlaceCreateDTO createDTO);
        (int Statuscode, string Message, int PlaceID) Update(PlaceEditDTO operationParameter);
        (int Statuscode, string Message) Delete(int placeID);
    }

    public class PlaceProcessor(IUnitOfWork unitOfWork
        , IProcessToponymy processToponymy
        , ITrackEventsCSV trackEvents) : IProcessPlace
    {
        public (int Statuscode, string Message, int PlaceID) Insert(PlaceCreateDTO createDTO)
        {
            try
            {
                using TransactionScope scope = new();

                Place newPlace = new()
                {
                    FurtherSpecs = createDTO.FurtherSpecs,
                    WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                };
                newPlace = unitOfWork.PlaceRepository.Insert(newPlace);
                unitOfWork.Save();

                foreach (ToponymyCreateDTO toponymyCreateDTO in createDTO.ToponymyList)
                {
                    ConnectToponymy(newPlace, toponymyCreateDTO);
                }
                foreach (ConnectedPlace connectedPlace in createDTO.ConnectedPlaceList)
                {
                    ConnectPlaceToPlace(newPlace, connectedPlace.PlaceID);
                }

                scope.Complete();
                return (201, "Success_Place_Created", newPlace.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "PlaceProcessor.Create: Error occurred while creating Place.", new Dictionary<string, object>
                {
                    { "Place", createDTO},
                    { "ToponymyList", createDTO.ToponymyList }
                });
                return (500, "Error_Unknown", new());
            }
        }

        public (int Statuscode, string Message) Delete(int placeID)
        {
            Place? placeToDelete = GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [placeID] }).FirstOrDefault();
            if (placeToDelete == null)
            {
                trackEvents.TrackError("PlaceProcessor.Delete: Place not found.", new Dictionary<string, object>
                    {
                        { "PlaceID", placeID}
                    });
                return (404, "Error_Place_NotFound");
            }
            if (placeToDelete.CollectionItemNPlaceList != null && placeToDelete.CollectionItemNPlaceList.Count > 0)
            {
                trackEvents.TrackError("PlaceProcessor.Delete: Place is connected to collection items.", new Dictionary<string, object>
                    {
                        { "PlaceID", placeID}
                    });
                return (400, "Error_Place_ConnectedToCollectionItems");
            }
            if (placeToDelete.ConnectedPlaces != null && placeToDelete.ConnectedPlaces.ToList().Count > 0)
            {
                trackEvents.TrackError("PlaceProcessor.Delete: Place is connected to other places.", new Dictionary<string, object>
                    {
                        { "PlaceID", placeID}
                    });
                return (400, "Error_Place_ConnectedToOtherPlaces");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                for (int i = placeToDelete.PlaceNToponymyList.Count - 1; i == 0; i--)
                {
                    DisconnectToponymy(placeToDelete, placeToDelete.PlaceNToponymyList[i].ToponymyID);
                }

                unitOfWork.PlaceRepository.Delete(placeToDelete);
                unitOfWork.Save();

                scope.Complete();
                return (200, "Success_Place_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "PlaceProcessor.Delete: Error occurred while deleting Place.", new Dictionary<string, object>
                {
                    { "PlaceID", placeID}
                });
                return (500, "Error_Unknown");
            }
        }

        public (int Statuscode, string Message, int PlaceID) Update(PlaceEditDTO editDTO)
        {
            PlaceSearchParameterModel placeSearchParameter = new()
            {
                PlaceID = [editDTO.PlaceID]
            };
            Place? existingPlace = GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                trackEvents.TrackError("PlaceProcessor.Edit: Place not found.", new Dictionary<string, object>
                {
                    { "Place", editDTO},
                    { "ToponymyList", editDTO.ToponymyList}
                });
                return (404, "Error_Place_NotFound", new());
            }
            try
            {
                using TransactionScope scope = new();

                bool isChanged = false;
                if (existingPlace.FurtherSpecs != editDTO.FurtherSpecs)
                {
                    existingPlace.FurtherSpecs = editDTO.FurtherSpecs;
                    isChanged = true;
                }
                string? wikipediaUrlWithoutSubdomain = editDTO.WikipediaUrl?.ChangeStringToUriToRemoveSubdomain();
                if (existingPlace.WikipediaUrl != wikipediaUrlWithoutSubdomain)
                {
                    existingPlace.WikipediaUrl = wikipediaUrlWithoutSubdomain;
                    isChanged = true;
                }
                if (isChanged)
                {
                    unitOfWork.Save();
                }
                SyncToponymy(existingPlace, editDTO.ToponymyList);
                SyncConnectedPlaces(existingPlace, editDTO.ConnectedPlaceList);

                scope.Complete();
                return (200, "Success_Place_Updated", existingPlace.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "PlaceProcessor.Edit: Error occurred while updating Place.", new Dictionary<string, object>
                {
                    { "Place", editDTO},
                    { "ToponymyList", editDTO.ToponymyList}
                });
                return (500, "Error_Unknown", new());
            }
        }

        public List<Place> GetListWithPredicate(PlaceSearchParameterModel placeSearchParameter)
        {
            IEnumerable<Place> placeIEnumerable = unitOfWork.PlaceRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Place>(placeSearchParameter),
                includeProperties: GetPlaceIncludeProperties());

            return [.. placeIEnumerable.OrderBy(p => p.PlaceNToponymyList
                .Where(t => t.IsCurrentName)
                .Select(t => t.Toponymy.ToponymyName)
                .FirstOrDefault())];
        }
        private static string GetPlaceIncludeProperties()
        {
            return
                $"{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                $"{nameof(Place.ConnectionsAsFirst)}.{nameof(PlaceNPlace.Place2)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                $"{nameof(Place.ConnectionsAsSecond)}.{nameof(PlaceNPlace.Place1)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                $"{nameof(Place.CollectionItemNPlaceList)}";
        }

        private void ConnectToponymy(Place place, ToponymyCreateDTO toponymyCreateDTO)
        {
            PlaceNToponymy newPlaceNToponymy = new()
            {
                PlaceID = place.PlaceID,
                IsCurrentName = toponymyCreateDTO.IsCurrentName,
                ToponymyID = processToponymy.Insert(toponymyCreateDTO.Name)
            };
            _ = unitOfWork.PlaceNToponomyRepository.Insert(newPlaceNToponymy);
            unitOfWork.Save();
        }
        private void SyncToponymy(Place place, List<PlaceNToponymyEditDTO> newConnections)
        {
            List<PlaceNToponymy> currentConnections = place.PlaceNToponymyList;

            for (int i = currentConnections.Count - 1; i == 0; i--)
            {
                PlaceNToponymyEditDTO? updatedConnection = newConnections.FirstOrDefault(x => x.Name == currentConnections[i].Toponymy.ToponymyName);
                if (updatedConnection == null)
                {
                    DisconnectToponymy(place, currentConnections[i].ToponymyID);
                }
                else if (updatedConnection.IsCurrentName != currentConnections[i].IsCurrentName)
                {
                    //Wenn sich Ortsname ändert, dann soll Eintrag gelöscht werden
                    UpdatePlaceNToponymy(place, currentConnections[i], updatedConnection.IsCurrentName);
                }
                // else: Beziehung ist gleich, keine Änderung notwendig
            }

            foreach (PlaceNToponymyEditDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.Toponymy.ToponymyName == newItem.Name);
                if (!exists)
                {
                    ToponymyCreateDTO toponymyCreateDTO = new()
                    {
                        Name = newItem.Name,
                        IsCurrentName = newItem.IsCurrentName
                    };
                    ConnectToponymy(place, toponymyCreateDTO);
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
            var placeNToponymyList = unitOfWork.PlaceNToponomyRepository.Get(
                filter: c => c.ToponymyID == toponymyID);
            if (placeNToponymyList == null)
            {
                return;
            }
            var placeNToponymy = placeNToponymyList.FirstOrDefault(x => x.PlaceID == place.PlaceID);
            if (placeNToponymy != null)
            {
                unitOfWork.PlaceNToponomyRepository.Delete(placeNToponymy);
                unitOfWork.Save();
            }

            if (placeNToponymyList.Count() <= 1)
                processToponymy.Delete(toponymyID);
        }

        private void ConnectPlaceToPlace(Place parentPlace, int otherPlaceId)
        {
            if (parentPlace.PlaceID == otherPlaceId)
                return;

            var place2 = unitOfWork.PlaceRepository.GetByID(otherPlaceId);
            if (place2 is null)
                return;

            int id1 = parentPlace.PlaceID;
            int id2 = otherPlaceId;

            // Symmetrie erzwingen
            if (id1 > id2)
                (id1, id2) = (id2, id1);

            bool alreadyExists = unitOfWork.PlaceNPlaceRepository
                .Get(p => p.PlaceID1 == id1 && p.PlaceID2 == id2)
                .Any();

            if (alreadyExists)
                return;

            var connection = new PlaceNPlace
            {
                PlaceID1 = id1,
                PlaceID2 = id2
            };

            unitOfWork.PlaceNPlaceRepository.Insert(connection);
            unitOfWork.Save();
        }
        private List<string> SyncConnectedPlaces(Place place, List<ConnectedPlace> newConnections)
        {
            List<Place> currentConnections = [.. place.ConnectedPlaces];

            for (int i = currentConnections.Count - 1; i == 0; i--)
            {
                ConnectedPlace? updated = newConnections.FirstOrDefault(x => x.PlaceID == currentConnections[i].PlaceID);

                if (updated == null)
                {
                    DisconnectPlaceConnection(place, currentConnections[i].PlaceID);
                }
            }

            List<string> translationList = [];
            foreach (ConnectedPlace newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PlaceID == newItem.PlaceID);
                if (!exists)
                {
                    if (newItem.PlaceID == place.PlaceID)
                    {
                        continue; // Ein Ort kann nicht mit sich selbst verbunden sein
                    }
                    ConnectPlaceToPlace(place, newItem.PlaceID);
                }
            }

            return translationList;
        }
        private void DisconnectPlaceConnection(Place place, int placeID)
        {
            PlaceNPlace? placeNPlace = (from pnp in unitOfWork.PlaceNPlaceRepository.Get(includeProperties: "Place")
                                        where pnp.PlaceID1 == place.PlaceID && pnp.PlaceID2 == placeID
                                        select pnp).FirstOrDefault();

            if (placeNPlace != null)
            {
                unitOfWork.PlaceNPlaceRepository.Delete(placeNPlace);
                unitOfWork.Save();
            }
        }
    }
}
