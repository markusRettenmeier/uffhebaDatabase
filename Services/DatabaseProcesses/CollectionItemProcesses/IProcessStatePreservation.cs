using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessStatePreservation
    {
        (int StatusCode, string StatusMessage, int Id) Insert(StatePreservationCreateDTO createDTO);
        (int StatusCode, string StatusMessage, int Id) Update(StatePreservationEditDTO editDto);
        (int StatusCode, string StatusMessage) Delete(int id);
        List<StatePreservation> GetWithPredicates(StatePreservationSearchParameterModel stateSearchParameterModel);
    }

    public class StatePreservationProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents) : IProcessStatePreservation
    {
        public (int StatusCode, string StatusMessage, int Id) Insert(StatePreservationCreateDTO createDto)
        {
            int? statePreservationID = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(StatePreservation)],
                FieldName = [nameof(StatePreservation.StatePreservationName)],
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
                        EntityType = nameof(StatePreservation),
                        EntityId = statePreservation.StatePreservationID,
                        FieldName = nameof(StatePreservation.StatePreservationName),
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name),
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
            var existingState = unitOfWork.StateRepository.GetByID(editDto.Id);
            if (existingState == null)
            {
                return (400, "Error_StatePreservation_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new();

                if (existingState.StatePreservationName != editDto.Name
                    || existingState.SortingOrder != editDto.SortingOrder)
                {
                    if (existingState.StatePreservationName != editDto.Name)
                    {
                        processTranslations.Update(
                            new TranslationDTO
                            {
                                TextToTranslate = editDto.Name,
                                EntityType = nameof(StatePreservation),
                                EntityId = existingState.StatePreservationID,
                                FieldName = nameof(StatePreservation.StatePreservationName),
                                Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name),
                            });
                    }
                    existingState.SortingOrder = editDto.SortingOrder;
                    unitOfWork.Save();
                }

                scope.Complete();
                return (200, "Success_StatePreservation_Updated", existingState.StatePreservationID);
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
            var existingState = GetWithPredicates(new StatePreservationSearchParameterModel { StatePreservationID = [id] }).FirstOrDefault();
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
                    EntityType = [nameof(StatePreservation)],
                    FieldName = [nameof(StatePreservation.StatePreservationName)],
                    EntityId = [id]
                });

            unitOfWork.StateRepository.Delete(existingState);
            unitOfWork.Save();

            return (200, "Success_StatePreservation_Deleted");
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
                    nameof(StatePreservation.StatePreservationName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
                state.CollectionArea.CollectionAreaName = translationStore.GetTranslation(
                    nameof(CollectionArea),
                    state.CollectionArea.CollectionAreaID,
                    nameof(CollectionArea.CollectionAreaName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
            }

            return [.. stateIEnumberable.OrderBy(x => x.SortingOrder)];
        }
    }
}
