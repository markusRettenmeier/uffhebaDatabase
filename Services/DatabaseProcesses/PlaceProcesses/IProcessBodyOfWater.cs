using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessBodyOfWater
    {
        (int PlaceID, int Statuscode, string Message) Create(BodyOfWaterOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) Edit(BodyOfWaterOperationParameterModel operationParameterModel);
        void DeleteBodyOfWater(int bodyOfWaterID);
    }
    public class BodyOfWaterProcessor(IProcessPlace processPlace,
                                      IUnitOfWork unitOfWork, 
                                      IProcessTranslations processTranslations) : IProcessBodyOfWater
    {
        public (int PlaceID, int Statuscode, string Message) Create(BodyOfWaterOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (0, 412, "Error_PlaceName_Missing");
            }
            (bool flowControl, (int PlaceID, int Statuscode, string Message) value) = IsPlaceExistingProcessCreate(operationParameterModel);
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
                (Place Place, int Statuscode, string Message) newPlace = processPlace.Create(placeOperationParameter);

                operationParameterModel.BodyOfWater.PlaceID = newPlace.Place.PlaceID;
                BodyOfWater newBodyOfWater = unitOfWork.BodyOfWaterRepository.Insert(operationParameterModel.BodyOfWater);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newBodyOfWater.PlaceID, 201, "Success_Place_Created");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Gewässer: {ex}", ex);
                return (0, 500, "Error_Error_Ocurred");
            }
        }
        private (bool flowControl, (int PlaceID, int Statuscode, string Message) value) IsPlaceExistingProcessCreate(BodyOfWaterOperationParameterModel operationParameterModel)
        {
            PlaceSearchParameter placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [operationParameterModel.Place.ToponymyTypeInt]
            };
            List<int> entityIdList = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
            {
                EntityType = [nameof(Toponymy)],
                TranslatedText = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)]
            }).Select(x => x.EntityId)];
            if (entityIdList.Count > 0)
            {
                placeSearchParameter.PlaceNToponymyList_Toponymy_ToponymyID = entityIdList;
            }
            Place? placeExists = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (placeExists != null)
            {
                return (flowControl: false, value: (placeExists.PlaceID, 409, "Error_Place_Exists"));
            }

            return (flowControl: true, value: default);
        }

        public void DeleteBodyOfWater(int bodyOfWaterID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) Edit(BodyOfWaterOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                return (new(), 412, "Error_PlaceID_Missing");
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                return (operationParameterModel.Place.PlaceID, 412, "Error_PlaceName_Missing");
            }

            BodyOfWater? existingBodyOfWater = processPlace
                .GetListWithPredicate(new PlaceSearchParameter { PlaceID = [operationParameterModel.Place.PlaceID] })
                .FirstOrDefault()?.BodyOfWater;
            if (existingBodyOfWater == null)
            {
                return (0, 404, "Error_Place_NotFound");
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
                return (existingBodyOfWater.PlaceID, 200, "Success_Place_Updated");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingBodyOfWater.PlaceID, 500, "Error_Error_Ocurred");
            }
        }
    }
}
