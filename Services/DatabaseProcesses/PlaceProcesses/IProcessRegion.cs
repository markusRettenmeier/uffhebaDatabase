using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessRegion
    {
        (int PlaceID, int Statuscode, string Message) CreateRegion(RegionOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditRegion(RegionOperationParameterModel operationParameterModel);
        void DeleteRegion(int regionID);
    }

    public class RegionProcessor(IProcessPlace processPlace,
                                IUnitOfWork unitOfWork,
                                IProcessTranslations processTranslations) : IProcessRegion
    {
        public (int PlaceID, int Statuscode, string Message) CreateRegion(RegionOperationParameterModel operationParameterModel)
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

                operationParameterModel.Region.PlaceID = newPlace.Place.PlaceID;
                Region newRegion = unitOfWork.RegionRepository.Insert(operationParameterModel.Region);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newRegion.PlaceID, 201, "Success_Place_Created");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Gewässer: {ex}", ex);
                return (0, 500, "Error_Error_Ocurred");
            }
        }
        private (bool flowControl, (int PlaceID, int Statuscode, string Message) value) IsPlaceExistingProcessCreate(RegionOperationParameterModel operationParameterModel)
        {
            PlaceSearchParameter placeSearchParameter = new()
            {
                PlaceNToponymyList_Toponymy_ToponymyName = [.. operationParameterModel.PlaceNToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)).Select(p => p.Toponymy.ToponymyName)],
                ToponymyTypeInt = [operationParameterModel.Place.ToponymyTypeInt],
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


        public void DeleteRegion(int regionID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) EditRegion(RegionOperationParameterModel operationParameterModel)
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

            Region? existingRegion = processPlace.GetListWithPredicate(new PlaceSearchParameter { PlaceID = [operationParameterModel.Place.PlaceID] }).FirstOrDefault()?.Region;
            if (existingRegion == null)
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
                return (existingRegion.PlaceID, 200, "Success_Place_Updated");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingRegion.PlaceID, 500, "Error_Error_Ocurred");
            }
        }
    }
}
