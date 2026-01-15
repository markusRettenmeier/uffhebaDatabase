using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessBuilding
    {
        (int Statuscode, string Message, int PlaceID) Insert(BuildingOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) Update(BuildingOperationParameterModel operationParameterModel);
    }

    public class BuildingProcessor(IProcessPlace processPlace,
                                   IUnitOfWork unitOfWork,
                                   IProcessTranslations processTranslations,
                                   ITrackEvents trackEvents) : IProcessBuilding
    {
        public (int Statuscode, string Message, int PlaceID) Insert(BuildingOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("BuildingProcessor.CreateBuilding: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Building", operationParameterModel.Building}
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

                operationParameterModel.Building.PlaceID = newPlace.Place.PlaceID;
                Building newBuilding = unitOfWork.BuildingRepository.Insert(operationParameterModel.Building);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newBuilding.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BuildingProcessor.CreateBuilding: Error occurred while creating Building.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Building", operationParameterModel.Building}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int Statuscode, string Message, int PlaceID) Update(BuildingOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("BuildingProcessor.EditBuilding: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Building", operationParameterModel.Building}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("BuildingProcessor.EditBuilding: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Building", operationParameterModel.Building}
                });
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            PlaceSearchParameterModel placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameterModel.Place.PlaceID);
            Building? existingBuilding = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.Building;
            if (existingBuilding == null)
            {
                trackEvents.TrackWarning("BuildingProcessor.EditBuilding: Building not found for the given PlaceID.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Building", operationParameterModel.Building}
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
                return (200, "Success_Place_Updated", existingBuilding.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BuildingProcessor.EditBuilding: Error occurred while editing Building.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "Building", operationParameterModel.Building}
                });
                return (500, "Error_Error_Ocurred", existingBuilding.PlaceID);
            }

        }
        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(BuildingOperationParameterModel operationParameterModel)
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
                trackEvents.TrackWarning("BuildingProcessor.IsPlaceExistingProcessCreate: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Building", operationParameterModel.Building}
                });
                return (flowControl: false, value: (409, "Error_Place_Exists", placeExists.PlaceID));
            }

            return (flowControl: true, value: default);
        }
    }
}
