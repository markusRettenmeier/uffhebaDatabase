using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessField
    {
        (int PlaceID, int Statuscode, string Message) CreateField(FieldOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditField(FieldOperationParameterModel operationParameterModel);
        void DeleteField(int fieldID);
    }

    public class FieldProcessor(IProcessPlace processPlace, IUnitOfWork unitOfWork) : IProcessField
    {
        public (int PlaceID, int Statuscode, string Message) CreateField(FieldOperationParameterModel operationParameterModel)
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

                operationParameterModel.Field.PlaceID = newPlace.Place.PlaceID;
                Field newField = unitOfWork.FieldRepository.Insert(operationParameterModel.Field);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newField.PlaceID, 201, "Ort erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen des Feldes: {ex}", ex);
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public void DeleteField(int fieldID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) EditField(FieldOperationParameterModel operationParameterModel)
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
            Field? existingField = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Field;
            if (existingField == null)
            {
                return (0, 404, "Flur nicht gefunden.");
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
                return (existingField.PlaceID, 200, "Flur erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingField.PlaceID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
    }
}
