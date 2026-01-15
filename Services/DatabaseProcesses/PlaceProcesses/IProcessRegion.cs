using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessRegion
    {
        (int Statuscode, string Message, int PlaceID) Insert(RegionOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) Update(RegionOperationParameterModel operationParameterModel);
        void DeleteRegion(int regionID);
    }

    public class RegionProcessor(IProcessPlace processPlace,
                                IUnitOfWork unitOfWork,
                                IProcessTranslations processTranslations,
                                ITrackEvents trackEvents) : IProcessRegion
    {
        public (int Statuscode, string Message, int PlaceID) Insert(RegionOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("RegionProcessor.CreateRegion: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
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

                operationParameterModel.Region.PlaceID = newPlace.Place.PlaceID;
                Region newRegion = unitOfWork.RegionRepository.Insert(operationParameterModel.Region);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newRegion.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "RegionProcessor.CreateRegion: Error occurred while creating region.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(RegionOperationParameterModel operationParameterModel)
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
                trackEvents.TrackWarning("RegionProcessor.IsPlaceExistingProcessCreate: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
                });
                return (flowControl: false, value: (409, "Error_Place_Exists", placeExists.PlaceID));
            }

            return (flowControl: true, value: default);
        }


        public void DeleteRegion(int regionID)
        {
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, int PlaceID) Update(RegionOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("RegionProcessor.EditRegion: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("RegionProcessor.EditRegion: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
                });
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            Region? existingRegion = processPlace.GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [operationParameterModel.Place.PlaceID] }).FirstOrDefault()?.Region;
            if (existingRegion == null)
            {
                trackEvents.TrackWarning("RegionProcessor.EditRegion: Region not found.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
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
                return (200, "Success_Place_Updated", existingRegion.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "RegionProcessor.EditRegion: Error occurred while editing region.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "Region", operationParameterModel.Region}
                });
                return (500, "Error_Error_Ocurred", existingRegion.PlaceID);
            }
        }
    }
}
