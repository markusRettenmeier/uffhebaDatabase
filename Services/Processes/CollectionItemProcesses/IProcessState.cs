using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;

namespace Sammlerplattform.Services.Processes.CollectionItemProcesses
{
    public interface IProcessState
    {
        (int CollectionAreaID, string StatusMessage) Create(State state);
        (int CollectionAreaID, string StatusMessage) Update(State state);
        (int CollectionAreaID, string StatusMessage) Delete(int stateID, int collectionAreaID);
        List<State> GetWithPredicates(StateSearchParameterModel stateSearchParameterModel);
    }

    public class StateProcessor(IUnitOfWork unitOfWork) : IProcessState
    {
        public (int CollectionAreaID, string StatusMessage) Create(State state)
        {
            int collectionAreaID = state.CollectionAreaID ?? 0;
            if (string.IsNullOrEmpty(state.StateName) || string.IsNullOrWhiteSpace(state.StateName) || state.CollectionAreaID <= 0)
            {
                return (collectionAreaID, "Ungültige Eingabedaten.");
            }


            if (state.IsGeneralState)
            {
                state.CollectionAreaID = null;
            }

            unitOfWork.StateRepository.Insert(state);
            unitOfWork.Save();

            return (collectionAreaID, "Zustand erfolgreich erstellt.");
        }

        public (int CollectionAreaID, string StatusMessage) Update(State state)
        {
            int collectionAreaID = state.CollectionAreaID ?? 0;
            if (string.IsNullOrEmpty(state.StateName) || string.IsNullOrWhiteSpace(state.StateName) || state.CollectionAreaID <= 0)
            {
                return (collectionAreaID, "Ungültige Eingabedaten.");
            }

            var existingState = unitOfWork.StateRepository.GetByID(state.StateID);
            if (existingState == null)
            {
                return (collectionAreaID, "Zustand nicht gefunden.");
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
                existingState.StateName = state.StateName;
                existingState.SortingOrder = state.SortingOrder;
                unitOfWork.Save();
            }

            return (collectionAreaID, "Zustand erfolgreich aktualisiert.");
        }

        public (int CollectionAreaID, string StatusMessage) Delete(int stateID, int collectionAreaID)
        {
            var existingState = unitOfWork.StateRepository.GetByID(stateID);
            if (existingState == null)
            {
                return (collectionAreaID, "Zustand nicht gefunden.");
            }

            unitOfWork.StateRepository.Delete(existingState);
            unitOfWork.Save();

            return (collectionAreaID, "Zustand erfolgreich gelöscht.");
        }

        public List<State> GetWithPredicates(StateSearchParameterModel stateSearchParameterModel)
        {
            IEnumerable<State> stateIEnumberable = unitOfWork.StateRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<State>(stateSearchParameterModel),
                includeProperties: "CollectionArea");

            return [.. stateIEnumberable.OrderBy(x => x.SortingOrder)];
        }
    }
}
