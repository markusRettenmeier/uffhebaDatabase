using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessBuilding
    {
        (int PlaceID, int Statuscode, string Message) CreateBuilding(BuildingOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditBuilding(BuildingOperationParameterModel operationParameterModel);
    }

    public class BuildingProcessor(IProcessPlace processPlace,
                                   IUnitOfWork unitOfWork) : IProcessBuilding
    {
        public (int PlaceID, int Statuscode, string Message) CreateBuilding(BuildingOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (0, 412, "Ortsnamen angeben.");
            }

            PlaceSearchParameter placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [operationParameterModel.Place.ToponymyTypeInt],
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
                    Place = operationParameterModel.Place,
                    PlaceNToponymyList = operationParameterModel.PlaceNToponymyList,
                    ChildPlaceList = operationParameterModel.ChildPlaceList
                };
                var newPlace = processPlace.CreatePlace(placeOperationParameter);

                operationParameterModel.Building.PlaceID = newPlace.Place.PlaceID;
                Building newBuilding = unitOfWork.BuildingRepository.Insert(operationParameterModel.Building);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newBuilding.PlaceID, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen des Gebäudes: {ex}", ex);
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (int PlaceID, int Statuscode, string Message) EditBuilding(BuildingOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                return (new(), 412, "Orts-ID ist leer.");
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (operationParameterModel.Place.PlaceID, 412, "Ortsnamen angeben.");
            }

            PlaceSearchParameter placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameterModel.Place.PlaceID);
            Building? existingBuilding = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Building;
            if (existingBuilding == null)
            {
                return (0, 404, "Ort nicht gefunden.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                PlaceOperationParameterModel placeOperationParameterModel = new()
                {
                    Place = operationParameterModel.Place,
                    PlaceNToponymyList = operationParameterModel.PlaceNToponymyList,
                    ChildPlaceList = operationParameterModel.ChildPlaceList
                };
                processPlace.EditPlace(placeOperationParameterModel);

                transactionScope.Complete();
                return (existingBuilding.PlaceID, 200, "Gewässer erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingBuilding.PlaceID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }

        }
    }
}
