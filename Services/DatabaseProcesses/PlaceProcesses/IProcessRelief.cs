using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessRelief
    {
        (int PlaceID, int Statuscode, string Message) CreateRelief(ReliefOperationParameterModel operationParameterModel);
        (int PlaceID, int Statuscode, string Message) EditRelief(ReliefOperationParameterModel operationParameterModel);
        void DeleteRelief(int reliefID);
    }

    public class ReliefProcessor(IProcessPlace processPlace,
                                      IUnitOfWork unitOfWork,
                                      IProcessTranslations processTranslations) : IProcessRelief
    {
        public (int PlaceID, int Statuscode, string Message) CreateRelief(ReliefOperationParameterModel operationParameterModel)
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

                operationParameterModel.Relief.PlaceID = newPlace.Place.PlaceID;
                Relief newRelief = unitOfWork.ReliefRepository.Insert(operationParameterModel.Relief);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newRelief.PlaceID, 201, "Success_Place_Created");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Hinzufügen der Relief: {ex}", ex);
                return (0, 500, "Error_Error_Ocurred");
            }
        }
        private (bool flowControl, (int PlaceID, int Statuscode, string Message) value) IsPlaceExistingProcessCreate(ReliefOperationParameterModel operationParameterModel)
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
            Place? existingPlace = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace != null)
            {
                return (false, (0, 409, "Error_Place_Exists"));
            }
            return (true, (0, 200, "Success_Place_NotExists"));
        }

        public void DeleteRelief(int reliefID)
        {
            throw new NotImplementedException();
        }

        public (int PlaceID, int Statuscode, string Message) EditRelief(ReliefOperationParameterModel operationParameterModel)
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

            Relief? existingRelief = processPlace
                .GetListWithPredicate(new PlaceSearchParameter { PlaceID = [operationParameterModel.Place.PlaceID] })
                .FirstOrDefault()?.Relief;
            if (existingRelief == null)
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
                return (existingRelief.PlaceID, 200, "Success_Place_Updated");
            }
            catch (Exception ex)
            {
                //logger.LogError("Fehler beim Aktualisieren der Gewässer: {ex}", ex);
                return (existingRelief.PlaceID, 500, "Error_Error_Ocurred");
            }
        }
    }
}
