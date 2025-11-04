using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessTransportRoute
    {
        (int PlaceID, int Statuscode, string Message) CreateTransportRoute(TransportRouteOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditTransportRoute(TransportRouteOperationParameterModel operationParameterModel);
        void DeleteTransportRoute(int transportRouteID);
    }
    public class TransportRouteProcessor(IProcessPlace processPlace,
                                       IUnitOfWork unitOfWork) : IProcessTransportRoute
    {
        public (int PlaceID, int Statuscode, string Message) CreateTransportRoute(TransportRouteOperationParameterModel operationParameterModel)
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
                (Place Place, int Statuscode, string Message) newPlace = processPlace.Create(placeOperationParameter);

                operationParameterModel.TransportRoute.PlaceID = newPlace.Place.PlaceID;
                TransportRoute newTransportRoute = unitOfWork.TransportRouteRepository.Insert(operationParameterModel.TransportRoute);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newTransportRoute.PlaceID, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Verkehrswege: {ex}", ex);
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public void DeleteTransportRoute(int transportRouteID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) EditTransportRoute(TransportRouteOperationParameterModel operationParameterModel)
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
            TransportRoute? existingTransportRoute = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.TransportRoute;
            if (existingTransportRoute == null)
            {
                return (0, 404, "Gewässer nicht gefunden.");
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
                _ = processPlace.Edit(placeOperationParameterModel);

                transactionScope.Complete();
                return (existingTransportRoute.PlaceID, 200, "Gewässer erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingTransportRoute.PlaceID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
    }
}
