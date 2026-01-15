using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessBodyOfWater
    {
        (int Statuscode, string Message, int PlaceID) Create(BodyOfWaterOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) Edit(BodyOfWaterOperationParameterModel operationParameterModel);
        void DeleteBodyOfWater(int bodyOfWaterID);
    }
    public class BodyOfWaterProcessor(IProcessPlace processPlace,
                                      IUnitOfWork unitOfWork, 
                                      IProcessTranslations processTranslations,
                                      ITrackEvents trackEvents) : IProcessBodyOfWater
    {
        public (int Statuscode, string Message, int PlaceID) Create(BodyOfWaterOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("BodyOfWaterProcessor.Create: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
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

                operationParameterModel.BodyOfWater.PlaceID = newPlace.Place.PlaceID;
                BodyOfWater newBodyOfWater = unitOfWork.BodyOfWaterRepository.Insert(operationParameterModel.BodyOfWater);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newBodyOfWater.PlaceID);
            }
            catch (Exception ex)
            {

                trackEvents.TrackException(ex, "BodyOfWaterProcessor.Create: Error occurred while creating BodyOfWater.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList},
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(BodyOfWaterOperationParameterModel operationParameterModel)
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
                trackEvents.TrackWarning("BodyOfWaterProcessor.Create: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList},
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
                });
                return (flowControl: false, value: (409, "Error_Place_Exists", placeExists.PlaceID));
            }

            return (flowControl: true, value: default);
        }

        public void DeleteBodyOfWater(int bodyOfWaterID)
        {
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, int PlaceID) Edit(BodyOfWaterOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("BodyOfWaterProcessor.Edit: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("BodyOfWaterProcessor.Edit: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
                });
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            BodyOfWater? existingBodyOfWater = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [operationParameterModel.Place.PlaceID] })
                .FirstOrDefault()?.BodyOfWater;
            if (existingBodyOfWater == null)
            {
                trackEvents.TrackWarning("BodyOfWaterProcessor.Edit: BodyOfWater not found.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList},
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
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
                return (200, "Success_Place_Updated", existingBodyOfWater.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BodyOfWaterProcessor.Edit: Error occurred while editing BodyOfWater.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList},
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "BodyOfWater", operationParameterModel.BodyOfWater}
                });
                return (500, "Error_Error_Ocurred", existingBodyOfWater.PlaceID);
            }
        }
    }
}
