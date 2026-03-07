using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessStatePreservation
    {
        (int StatusCode, string StatusMessage) Insert(StatePreservation state);
        (int StatusCode, string StatusMessage) Update(StatePreservation state);
        (int StatusCode, string StatusMessage) Delete(int id);
        List<StatePreservation> GetWithPredicates(StatePreservationSearchParameterModel stateSearchParameterModel);
    }

    public class StatePreservationProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents) : IProcessStatePreservation
    {
        public (int StatusCode, string StatusMessage) Insert(StatePreservation state)
        {
            int collectionAreaID = state.CollectionAreaID ?? 0;
            if (string.IsNullOrEmpty(state.StatePreservationName) || string.IsNullOrWhiteSpace(state.StatePreservationName) || state.CollectionAreaID <= 0)
            {
                trackEvents.TrackError("StateProcessor/Create: StateName or CollectionAreaID is missing.", new Dictionary<string, object>
                {
                    {"State", state }
                });
                return (400, "Error_CollectionArea_IdMissing");
            }

            StatePreservationSearchParameterModel searchParameterModel = new() {
                StatePreservationID = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(StatePreservation)],
                        FieldName = [nameof(StatePreservation.StatePreservationName)],
                        TranslatedText = [state.StatePreservationName]
                    }).Select(x => x.EntityId).Distinct()]
            };
            if (searchParameterModel.StatePreservationID.Count > 0)
            {                
                trackEvents.TrackError("StateProcessor/Create: State already exists.", new Dictionary<string, object>
                {
                    {"State", state }
                });
                return (301, "Error_StatePreservation_Exists");
            }

            try
            {
                TransactionScope scope = new();

                if (state.IsGeneralState)
                {
                    state.CollectionAreaID = null;
                }

                unitOfWork.StateRepository.Insert(state);
                unitOfWork.Save();

                processTranslations.Insert(
                    new Models.Translations.EntityTranslation
                    {
                        EntityType = nameof(StatePreservation),
                        EntityId = state.StatePreservationID,
                        FieldName = nameof(StatePreservation.StatePreservationName),
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name),
                        TranslatedText = state.StatePreservationName
                    }, state.StatePreservationName);

                scope.Complete();
                return (200, "Success_StatePreservation_Created");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "StateProcessor/Create: Exception occurred while creating state.", new Dictionary<string, object>
                {
                    {"State", state }
                });
                return (500, "Error_StatePreservation_CreationFailed");
            }
        }

        public (int StatusCode, string StatusMessage) Update(StatePreservation state)
        {
            if (string.IsNullOrEmpty(state.StatePreservationName) || string.IsNullOrWhiteSpace(state.StatePreservationName) || state.CollectionAreaID <= 0)
            {
                return (400, "Error_CollectionArea_IdMissing");
            }

            var existingState = unitOfWork.StateRepository.GetByID(state.StatePreservationID);
            if (existingState == null)
            {
                return (400, "Error_StatePreservation_NotFound");
            }

            try
            {
                TransactionScope scope = new();
                if (existingState.IsGeneralState != state.IsGeneralState
                    || existingState.StatePreservationName != state.StatePreservationName
                    || existingState.CollectionAreaID != state.CollectionAreaID
                    || existingState.SortingOrder != state.SortingOrder)
                {
                    existingState.IsGeneralState = state.IsGeneralState;
                    if (existingState.IsGeneralState)
                    {
                        existingState.CollectionAreaID = null;
                    }
                    else
                    {
                        existingState.CollectionAreaID = state.CollectionAreaID;
                    }
                    if (existingState.StatePreservationName != state.StatePreservationName)
                    {
                        processTranslations.Update(
                            new Models.Translations.EntityTranslation
                            {
                                EntityType = nameof(StatePreservation),
                                EntityId = state.StatePreservationID,
                                FieldName = nameof(StatePreservation.StatePreservationName),
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name),
                                TranslatedText = state.StatePreservationName
                            }, state.StatePreservationName);
                    }
                    existingState.SortingOrder = state.SortingOrder;
                    unitOfWork.Save();
                }

                scope.Complete();
                return (200, "Success_StatePreservation_Updated");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "StateProcessor/Update: Exception occurred while updating state.", new Dictionary<string, object>
                {
                    {"State", state }
                });
                return (500, "Error_StatePreservation_UpdateFailed");
            }
        }

        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            var existingState = GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).FirstOrDefault();
            if (existingState == null)
            {
                return (400, "Error_StatePreservation_NotFound");
            }
            if (existingState.CollectionItemEntityList != null && existingState.CollectionItemEntityList.Count > 0)
            {
                return (400, "Error_StatePreservation_InUse");
            }
            if (existingState.IsGeneralState)
            {
                return (400, "Error_StatePreservation_GeneralState");
            }

            unitOfWork.StateRepository.Delete(existingState);
            unitOfWork.Save();

            return (400, "Success_StatePreservation_Deleted");
        }

        public List<StatePreservation> GetWithPredicates(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            IEnumerable<StatePreservation> stateIEnumberable = unitOfWork.StateRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<StatePreservation>(stateSearchParameterModel),
                includeProperties: nameof(StatePreservation.CollectionArea));
            List<StatePreservation> stateList = [.. stateIEnumberable];
            foreach (var state in stateList) 
            {
                state.StatePreservationName = translationStore.GetTranslation(
                    nameof(StatePreservation),
                    state.StatePreservationID,
                    state.StatePreservationName,
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? state.StatePreservationName;
            }

            return [.. stateIEnumberable.OrderBy(x => x.SortingOrder)];
        }
    }
}
