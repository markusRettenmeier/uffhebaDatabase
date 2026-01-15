using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessField
    {
        (int Statuscode, string Message, int PlaceID) Insert(FieldOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) Update(FieldOperationParameterModel operationParameterModel);
        void Delete(int fieldID);
    }

    public class FieldProcessor(IProcessPlace processPlace, 
        IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITrackEvents trackEvents) : IProcessField
    {
        public (int Statuscode, string Message, int PlaceID) Insert(FieldOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("FieldProcessor.Create: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Field", operationParameterModel.Field}
                });
                return (412, "Error_PlaceName_Missing", 0);
            }

            (bool flowControl, (int Statuscode, string Message, int PlaceID) value) = IsPlaceExistingProcessCreate(operationParameterModel);
            if (!flowControl)
            {
                return value;
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
                (int Statuscode, string Message, Place Place) newPlace = processPlace.Insert(placeOperationParameter);

                operationParameterModel.Field.PlaceID = newPlace.Place.PlaceID;
                Field newField = unitOfWork.FieldRepository.Insert(operationParameterModel.Field);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newField.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "FieldProcessor.Create: Error occurred while creating Field.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Field", operationParameterModel.Field}
                });
                return (500, "Error-Error_Ocurred", 0);
            }
        }

        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(FieldOperationParameterModel operationParameterModel)
        {
            PlaceSearchParameterModel placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [operationParameterModel.Place.ToponymyTypeInt],
                PlaceNToponymyList_Toponymy_ToponymyID = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(Toponymy)],
                        TranslatedText = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)]
                    }).Select(x => x.EntityId)]
            };
            if (placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID.Count == 0)
            {
                placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID = [0];
            }
            Place? placeExists = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (placeExists != null)
            {
                return (flowControl: false, value: (409, "Error_Place_Exists", placeExists.PlaceID));
            }

            return (flowControl: true, value: default);
        }

        public void Delete(int fieldID)
        {
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, int PlaceID) Update(FieldOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            Field? existingField = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [operationParameterModel.Place.PlaceID] })
                .FirstOrDefault()?.Field;
            if (existingField == null)
            {
                return (404, "Error_Place_NotFound", 0);
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
                _ = processPlace.Update(placeOperationParameterModel);

                transactionScope.Complete();
                return (200, "Success_Place_Updated", existingField.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "FieldProcessor.Edit: Error occurred while editing Field.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Field", operationParameterModel.Field}
                });
                return (500, "Error_Error_Ocurred", existingField.PlaceID);
            }
        }
    }
}
