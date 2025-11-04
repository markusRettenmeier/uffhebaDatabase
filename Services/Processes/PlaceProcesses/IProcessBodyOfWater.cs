using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessBodyOfWater
    {
        (int PlaceID, int Statuscode, string Message) CreateBodyOfWater(BodyOfWaterOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditBodyOfWater(BodyOfWaterOperationParameterModel operationParameterModel);
        void DeleteBodyOfWater(int bodyOfWaterID);
    }
    public class BodyOfWaterProcessor(IProcessPlace processPlace,
                                      IUnitOfWork unitOfWork) : IProcessBodyOfWater
    {
        public (int PlaceID, int Statuscode, string Message) CreateBodyOfWater(BodyOfWaterOperationParameterModel operationParameterModel)
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

                operationParameterModel.BodyOfWater.PlaceID = newPlace.Place.PlaceID;
                BodyOfWater newBodyOfWater = unitOfWork.BodyOfWaterRepository.Insert(operationParameterModel.BodyOfWater);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newBodyOfWater.PlaceID, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Gewässer: {ex}", ex);
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public void DeleteBodyOfWater(int bodyOfWaterID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) EditBodyOfWater(BodyOfWaterOperationParameterModel operationParameterModel)
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
            BodyOfWater? existingBodyOfWater = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.BodyOfWater;
            if (existingBodyOfWater == null)
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
                return (existingBodyOfWater.PlaceID, 200, "Gewässer erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingBodyOfWater.PlaceID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
    }
}
