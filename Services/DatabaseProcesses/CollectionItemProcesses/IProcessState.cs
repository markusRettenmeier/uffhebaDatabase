using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessState
    {
        (int CollectionAreaID, string StatusMessage) Create(State state);
        (int CollectionAreaID, string StatusMessage) Update(State state);
        (int CollectionAreaID, string StatusMessage) Delete(int stateID, int collectionAreaID);
        List<State> GetWithPredicates(StateSearchParameterModel stateSearchParameterModel);
    }

    public class StateProcessor(IUnitOfWork unitOfWork,
        DeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore) : IProcessState
    {
        public (int CollectionAreaID, string StatusMessage) Create(State state)
        {
            int collectionAreaID = state.CollectionAreaID ?? 0;
            if (string.IsNullOrEmpty(state.StateName) || string.IsNullOrWhiteSpace(state.StateName) || state.CollectionAreaID <= 0)
            {
                return (collectionAreaID, "Error_CollectionAreaID_Missing");
            }

            StateSearchParameterModel searchParameterModel = new() { StateName = [state.StateName] };
            List<int> entityIdList = [.. processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
            {
                EntityType = [nameof(State)],
                FieldName = [nameof(State.StateName)],
                TranslatedText = [state.StateName]
            }).Select(x => x.EntityId)];
            if (entityIdList.Count > 0)
            {
                searchParameterModel.StateID = entityIdList;
            }
            State? existingState = GetWithPredicates(searchParameterModel).FirstOrDefault();

            if (existingState != null)
            {
                return (collectionAreaID, "Error_State_AlreadyExists");
            }

            if (state.IsGeneralState)
            {
                state.CollectionAreaID = null;
            }
            state.StateName = translationService.SetIntoFallbackLanguage(state.StateName);

            unitOfWork.StateRepository.Insert(state);
            unitOfWork.Save();

            processTranslations.Create(
                new Models.Translations.EntityTranslation
                {
                    EntityType = nameof(State),
                    EntityId = state.StateID,
                    FieldName = nameof(State.StateName),
                    Culture = "de",
                    TranslatedText = state.StateName
                }, state.StateName);

            return (collectionAreaID, "Success_State_Created");
        }

        public (int CollectionAreaID, string StatusMessage) Update(State state)
        {
            int collectionAreaID = state.CollectionAreaID ?? 0;
            if (string.IsNullOrEmpty(state.StateName) || string.IsNullOrWhiteSpace(state.StateName) || state.CollectionAreaID <= 0)
            {
                return (collectionAreaID, "Error_CollectionAreaID_Missing");
            }

            var existingState = unitOfWork.StateRepository.GetByID(state.StateID);
            if (existingState == null)
            {
                return (collectionAreaID, "Error_State_NotFound");
            }

            if (existingState.IsGeneralState != state.IsGeneralState
                || existingState.StateName != state.StateName
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
                if(existingState.StateName != state.StateName)
                    existingState.StateName = translationService.SetIntoFallbackLanguage(state.StateName);
                existingState.SortingOrder = state.SortingOrder;
                unitOfWork.Save();
            }

            return (collectionAreaID, "Success_State_Updated");
        }

        public (int CollectionAreaID, string StatusMessage) Delete(int stateID, int collectionAreaID)
        {
            var existingState = unitOfWork.StateRepository.GetByID(stateID);
            if (existingState == null)
            {
                return (collectionAreaID, "Error_State_NotFound");
            }

            unitOfWork.StateRepository.Delete(existingState);
            unitOfWork.Save();

            return (collectionAreaID, "Success_State_Deleted");
        }

        public List<State> GetWithPredicates(StateSearchParameterModel stateSearchParameterModel)
        {
            IEnumerable<State> stateIEnumberable = unitOfWork.StateRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<State>(stateSearchParameterModel),
                includeProperties: "CollectionArea");
            List<State> stateList = [.. stateIEnumberable];
            foreach (var state in stateList) 
            {
                state.StateName = translationStore.GetTranslation(
                    nameof(State),
                    state.StateID,
                    state.StateName,
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? state.StateName;
            }

            return [.. stateIEnumberable.OrderBy(x => x.SortingOrder)];
        }
    }
}
