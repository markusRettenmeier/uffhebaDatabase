using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessTransportRoute
    {
        (int Statuscode, string Message, int PlaceID) Insert(TransportRouteOperationParameterModel operationParameterModel);
        (int Statuscode, string Message, int PlaceID) Update(TransportRouteOperationParameterModel operationParameterModel);
        void DeleteTransportRoute(int transportRouteID);
    }
    public class TransportRouteProcessor(IProcessPlace processPlace,
                                       IUnitOfWork unitOfWork,
                                       IProcessTranslations processTranslations,
                                       ITrackEvents trackEvents) : IProcessTransportRoute
    {
        public (int Statuscode, string Message, int PlaceID) Insert(TransportRouteOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("TransportRouteProcessor.CreateTransportRoute: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
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

                operationParameterModel.TransportRoute.PlaceID = newPlace.Place.PlaceID;
                TransportRoute newTransportRoute = unitOfWork.TransportRouteRepository.Insert(operationParameterModel.TransportRoute);
                unitOfWork.Save();

                transactionScope.Complete();
                return (201, "Success_Place_Created", newTransportRoute.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "TransportRouteProcessor.CreateTransportRoute: Error occurred while creating TransportRoute.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
        private (bool flowControl, (int Statuscode, string Message, int PlaceID) value) IsPlaceExistingProcessCreate(TransportRouteOperationParameterModel operationParameterModel)
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
                trackEvents.TrackWarning("TransportRouteProcessor.IsPlaceExistingProcessCreate: Place already exists.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "PlaceNToponymyList", operationParameterModel.PlaceNToponymyList },
                    { "Toponymy", operationParameterModel.PlaceNToponymyList.Select(x => x.Toponymy)},
                    { "TransportRoute", operationParameterModel.TransportRoute}
                });
                return (flowControl: false, value: (409, "Error_Place_Exists", placeExists.PlaceID));
            }

            return (flowControl: true, value: default);
        }

        public void DeleteTransportRoute(int transportRouteID)
        {
            throw new NotImplementedException();
        }

        public (int Statuscode, string Message, int PlaceID) Update(TransportRouteOperationParameterModel operationParameterModel)
        {
            if (operationParameterModel.Place.PlaceID == 0)
            {
                trackEvents.TrackWarning("TransportRouteProcessor.EditTransportRoute: PlaceID is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
                });
                return (412, "Error_PlaceID_Missing", new());
            }
            if (operationParameterModel.PlaceNToponymyList == null ||
                !operationParameterModel.PlaceNToponymyList.Any(x => x.Toponymy != null && !string.IsNullOrWhiteSpace(x.Toponymy.ToponymyName)))
            {
                trackEvents.TrackWarning("TransportRouteProcessor.EditTransportRoute: PlaceName is missing.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
                });
                return (412, "Error_PlaceName_Missing", operationParameterModel.Place.PlaceID);
            }

            PlaceSearchParameterModel placeSearchParameter = new();
            placeSearchParameter.PlaceID.Add(operationParameterModel.Place.PlaceID);
            TransportRoute? existingTransportRoute = processPlace.GetListWithPredicate(placeSearchParameter).FirstOrDefault()?.TransportRoute;
            if (existingTransportRoute == null)
            {
                trackEvents.TrackWarning("TransportRouteProcessor.EditTransportRoute: TransportRoute not found.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
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
                return (200, "Success_Place_Updated", existingTransportRoute.PlaceID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "TransportRouteProcessor.EditTransportRoute: Error occurred while editing TransportRoute.", new Dictionary<string, object>
                {
                    { "Place", operationParameterModel.Place},
                    { "TransportRoute", operationParameterModel.TransportRoute}
                });
                return (500, "Error_Error_Ocurred", existingTransportRoute.PlaceID);
            }
        }
    }
}
