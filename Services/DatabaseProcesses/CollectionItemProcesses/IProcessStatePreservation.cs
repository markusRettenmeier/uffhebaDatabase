using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.Translations;
using System.Data.Entity;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessStatePreservation
    {
        (int StatusCode, string StatusMessage, int Id) Insert(StatePreservationCreateDTO createDTO);
        (int StatusCode, string StatusMessage, int Id) Update(StatePreservationEditDTO editDto);
        (int StatusCode, string StatusMessage) Delete(int id);
        List<StatePreservation> GetEntityListViaPredicates(StatePreservationSearchParameterModel stateSearchParameterModel);
        List<StatePreservationDisplayDTO> GetWithTranslationsListViaPredicates(StatePreservationSearchParameterModel stateSearchParameterModel);
    }

    public class StatePreservationProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITrackEventsText trackEvents) : IProcessStatePreservation
    {
        public (int StatusCode, string StatusMessage, int Id) Insert(StatePreservationCreateDTO createDto)
        {
            int? statePreservationID = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(StatePreservation)],
                PropertyName = [nameof(StatePreservationDisplayDTO.Name)],
                TranslatedText = [createDto.Name]
            }).Select(x => x.EntityId).FirstOrDefault();
            if (statePreservationID > 0)
            {
                trackEvents.TrackError("StateProcessor/Create: State already exists.", new Dictionary<string, object>
                {
                    {"State", createDto }
                });
                return (301, "Error_StatePreservation_Exists", (int)statePreservationID);
            }

            try
            {
                using TransactionScope scope = new();

                StatePreservation statePreservation = new()
                {
                    CollectionAreaID = createDto.CollectionAreaID,
                    SortingOrder = createDto.SortingOrder
                };
                statePreservation = unitOfWork.StateRepository.Insert(statePreservation);
                unitOfWork.Save();

                processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDto.Name,
                        EntityName = nameof(StatePreservation),
                        EntityId = statePreservation.StatePreservationID,
                        PropertyName = nameof(StatePreservationDisplayDTO.Name)
                    });

                scope.Complete();
                return (200, "Success_StatePreservation_Created", statePreservation.StatePreservationID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "StateProcessor/Create: Exception occurred while creating state.", new Dictionary<string, object>
                {
                    {"State", createDto }
                });
                return (500, "Error_StatePreservation_CreationFailed", 0);
            }
        }

        public (int StatusCode, string StatusMessage, int Id) Update(StatePreservationEditDTO editDto)
        {
            var existingState = GetWithTranslationsListViaPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [editDto.Id] }).FirstOrDefault();
            if (existingState == null)
            {
                return (400, "Error_StatePreservation_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new();

                if (existingState.Name != editDto.Name
                    || existingState.SortingOrder != editDto.SortingOrder)
                {
                    if (existingState.Name != editDto.Name)
                    {
                        processTranslations.Update(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.Name,
                                EntityName = nameof(StatePreservation),
                                EntityId = existingState.Id,
                                PropertyName = nameof(StatePreservationDisplayDTO.Name)
                            });
                    }
                    existingState.SortingOrder = editDto.SortingOrder;
                    unitOfWork.Save();
                }

                scope.Complete();
                return (200, "Success_StatePreservation_Updated", existingState.Id);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "StateProcessor/Update: Exception occurred while updating state.", new Dictionary<string, object>
                {
                    {"State", editDto }
                });
                return (500, "Error_StatePreservation_UpdateFailed", 0);
            }
        }

        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            var existingState = GetEntityListViaPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).FirstOrDefault();
            if (existingState == null)
            {
                return (400, "Error_StatePreservation_NotFound");
            }
            if (existingState.CollectionItemEntityList != null && existingState.CollectionItemEntityList.Count > 0)
            {
                return (400, "Error_StatePreservation_InUse");
            }

            processTranslations.Delete(
                new EntityTranslationSearchParameter
                {
                    EntityName = [nameof(StatePreservation)],
                    PropertyName = [nameof(StatePreservationDisplayDTO.Name)],
                    EntityId = [id]
                });

            unitOfWork.StateRepository.Delete(existingState);
            unitOfWork.Save();

            return (200, "Success_StatePreservation_Deleted");
        }

        public List<StatePreservation> GetEntityListViaPredicates(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            IEnumerable<StatePreservation> stateIEnumberable = unitOfWork.StateRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<StatePreservation>(stateSearchParameterModel),
                orderBy: q => q.OrderBy(x => x.SortingOrder),
                includeProperties: nameof(StatePreservation.CollectionArea));

            return [.. stateIEnumberable];
        }

        public List<StatePreservationDisplayDTO> GetWithTranslationsListViaPredicates(StatePreservationSearchParameterModel stateSearchParameterModel)
        {
            List<StatePreservationDisplayDTO> stateList = [.. unitOfWork.StateRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<StatePreservation>(stateSearchParameterModel),
                orderBy: q => q.OrderBy(x => x.SortingOrder),
                includeProperties: nameof(StatePreservation.CollectionItemEntityList))
                .AsNoTracking()
                .Select(s => new StatePreservationDisplayDTO
                {
                    Id = s.StatePreservationID,
                    CollectionAreaID = s.CollectionAreaID,
                })];
            SetTranslations(stateList);

            return stateList;
        }

        private void SetTranslations(List<StatePreservationDisplayDTO> stateList)
        {
            var allStateOfPreservationTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(StatePreservation)],
                PropertyName = [nameof(StatePreservationDisplayDTO.Name)]
            }).ToList();
            foreach (var state in stateList)
            {
                state.Name = allStateOfPreservationTranslations.FirstOrDefault(t => t.EntityId == state.Id)?.TranslatedText ?? string.Empty;
            }
        }
    }
}
