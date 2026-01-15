using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessPlace
    {
        List<Place> GetListWithPredicate(PlaceSearchParameterModel placeSearchParameter);
        (int Statuscode, string Message, Place Place) Insert(PlaceOperationParameterModel operationParameter);
        (int Statuscode, string Message, Place Place) Update(PlaceOperationParameterModel operationParameter);
        (int Statuscode, string Message) Delete(int placeID);
    }

    public class PlaceProcessor(IUnitOfWork unitOfWork,
        IProcessToponymy processToponymy
        , IDeeplTranslationService translationService
        , IProcessTranslations processTranslations
        , ITranslationStore translationStore
        , ITrackEvents trackEvents) : IProcessPlace
    {
        public (int Statuscode, string Message, Place Place) Insert(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null ||
                    !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("PlaceProcessor.Create: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameter.Place}
                });
                return (412, "Error_PlaceName_Missing", new());
            }

            if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
            {
                trackEvents.TrackWarning("PlaceProcessor.Create: ParentPlaceID is the same as PlaceID.", new Dictionary<string, object>
                {
                    { "Place", operationParameter.Place},
                    { "PlaceNToponymyList", operationParameter.PlaceNToponymyList}
                });
                return (412, "Error_ParentPlace_ParentOfItsOwn", new());
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
                return (201, "Success_Place_Created", newPlace);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "PlaceProcessor.Create: Error occurred while creating Place.", new Dictionary<string, object>
                {
                    { "Place", operationParameter.Place},
                    { "PlaceNToponymyList", operationParameter.PlaceNToponymyList },
                    { "Toponymy", operationParameter.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "ChildPlaceList", operationParameter.ChildPlaceList}
                });
                return (500, "Error_Error_Ocurred", new());
            }
        }

        public (int Statuscode, string Message) Delete(int placeID)
        {
            if (placeID <= 0)
            {
                trackEvents.TrackWarning("PlaceProcessor.Delete: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "PlaceID", placeID}
                });
                return (400, "Error_PlaceID_Missing");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Place? placeToDelete = unitOfWork.PlaceRepository.GetByID(placeID);
                if (placeToDelete == null)
                {
                    trackEvents.TrackWarning("PlaceProcessor.Delete: Place not found.", new Dictionary<string, object>
                    {
                        { "PlaceID", placeID}
                    });
                    return (404, "Error_Place_NotFound");
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
                return (500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string Message, Place Place) Update(PlaceOperationParameterModel operationParameter)
        {
            if (operationParameter.PlaceNToponymyList == null || !operationParameter.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("PlaceProcessor.Edit: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameter.Place}
                });
                return (412, "Error_PlaceName_Missing", new());
            }

            PlaceSearchParameterModel placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameter.Place.PlaceID);
            Place? existingPlace = GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                trackEvents.TrackWarning("PlaceProcessor.Edit: Place not found.", new Dictionary<string, object>
                {
                    { "Place", operationParameter.Place},
                    { "PlaceNToponymyList", operationParameter.PlaceNToponymyList}
                });
                return (404, "Error_Place_NotFound", new());
            }

            bool isChanged = false;
            if (existingPlace.ParentPlaceID != operationParameter.Place.ParentPlaceID && operationParameter.Place.ParentPlaceID > 0)
            {
                if (operationParameter.Place.ParentPlaceID > 0 && operationParameter.Place.ParentPlaceID == operationParameter.Place.PlaceID)
                {
                    return (412, "Error_ParentPlace_ParentOfItsOwn", new());
                }
                existingPlace.ParentPlaceID = operationParameter.Place.ParentPlaceID;
            }
            if (existingPlace.ToponymyTypeInt != operationParameter.Place.ToponymyTypeInt)
            {
                existingPlace.ToponymyTypeInt = operationParameter.Place.ToponymyTypeInt;
                isChanged = true;
            }
            if (existingPlace.WikipediaUrl != operationParameter.Place.WikipediaUrl)
            {
                existingPlace.WikipediaUrl = operationParameter.Place.WikipediaUrl;
                isChanged = true;
            }
            if (isChanged)
            {
                unitOfWork.Save();
            }
            SyncChildPlaces(existingPlace, operationParameter.ChildPlaceList);
            SyncToponymy(existingPlace, operationParameter.PlaceNToponymyList);

            return (200, "Success_Party_Updated", existingPlace);
        }
        public List<Place> GetListWithPredicate(PlaceSearchParameterModel placeSearchParameter)
        {
            IEnumerable<Place> placeIEnumerable = unitOfWork.PlaceRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Place>(placeSearchParameter),
                includeProperties: GetPlaceIncludeProperties());

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
        private static string GetPlaceIncludeProperties()
        {
            return $"{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                   $"{nameof(Place.ParentPlace)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                   $"{nameof(Place.ParentPlace)}.{nameof(Place.Settlement)}.{nameof(Settlement.SettlementNPostalcodeList)}.{nameof(SettlementNPostalcode.Postalcode)}," +
                   $"{nameof(Place.ChildPlaceList)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                   $"{nameof(Place.ChildPlaceList)}.{nameof(Place.Settlement)}.{nameof(Settlement.SettlementNPostalcodeList)}.{nameof(SettlementNPostalcode.Postalcode)}," +
                   $"{nameof(Place.Settlement)}.{nameof(Settlement.SettlementNPostalcodeList)}.{nameof(SettlementNPostalcode.Postalcode)}," +
                   $"{nameof(Place.Settlement)}.{nameof(Settlement.RelatedGeography)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                   $"{nameof(Place.RelatedSettlement)}," +
                   $"{nameof(Place.BodyOfWater)}," +
                   $"{nameof(Place.Building)}," +
                   $"{nameof(Place.Field)}," +
                   $"{nameof(Place.Region)}," +
                   $"{nameof(Place.Relief)}," +
                   $"{nameof(Place.TransportRoute)}";
        }

        private void ConnectToponymy(Place place, PlaceNToponymy placeNToponymy)
        {
            if (string.IsNullOrWhiteSpace(placeNToponymy.Toponymy.ToponymyName))
            {
                return;
            }

            Toponymy newToponymy = new() { ToponymyName = ""};
            newToponymy = processToponymy.CreateOrEditToponymy(newToponymy);

            PlaceNToponymy newPlaceNToponymy = new()
            {
                PlaceID = place.PlaceID,
                ToponymyID = newToponymy.ToponymyID,
                IsCurrentName = placeNToponymy.IsCurrentName
            };
            _ = unitOfWork.PlaceNToponomyRepository.Insert(newPlaceNToponymy);
            unitOfWork.Save();

            processTranslations.Insert(
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
