using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessRelief
    {
        (int Statuscode, string Message, int PlaceID) CreateRelief(ReliefOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) EditRelief(ReliefOperationParameterModel operationParameterModel);
        void DeleteRelief(int reliefID);
    }

    public class ReliefProcessor(IProcessPlace processPlace,
                                      IUnitOfWork unitOfWork,
                                      IProcessTranslations processTranslations,
                                      ITrackEvents trackEvents) : IProcessRelief
    {
        public (int Statuscode, string Message, int PlaceID) CreateRelief(ReliefOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("ReliefProcessor.CreateRelief: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
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

                operationParameterModel.Relief.PlaceID = newPlace.Place.PlaceID;
                Relief newRelief = unitOfWork.ReliefRepository.Insert(operationParameterModel.Relief);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newRelief.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ReliefProcessor.CreateRelief: Error occurred while creating Relief.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(ReliefOperationParameterModel operationParameterModel)
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
            Place? existingPlace = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault();
            if (existingPlace != null)
            {
                trackEvents.TrackWarning("ReliefProcessor.IsPlaceExistingProcessCreate: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
                return (false, (409, "Error_Place_Exists", 0));
            }
            return (true, (200, "Success_Place_NotExists", 0));
        }

        public void DeleteRelief(int reliefID)
        {
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, int PlaceID) EditRelief(ReliefOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("ReliefProcessor.EditRelief: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("ReliefProcessor.EditRelief: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            Relief? existingRelief = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [operationParameterModel.Place.PlaceID] })
                .FirstOrDefault()?.Relief;
            if (existingRelief == null)
            {
                trackEvents.TrackWarning("ReliefProcessor.EditRelief: Relief not found for the given PlaceID.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
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
                return (200, "Success_Place_Updated", existingRelief.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ReliefProcessor.EditRelief: Error occurred while editing Relief.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Relief", operationParameterModel.Relief}
                });
                return (500, "Error_Error_Ocurred", existingRelief.PlaceID);
            }
        }
    }
}
