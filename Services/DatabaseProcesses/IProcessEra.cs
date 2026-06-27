using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Extensions;
using System.Data.Entity;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessEra
    {
        List<Era> GetEntityListViaPredicates(EraSearchParameterModel eraSearchParameter);
        List<EraDisplayDTO> GetTranslationsListViaPredicates(EraSearchParameterModel eraSearchParameter);
        (int statuscode, string message, int EraId) Insert(EraCreateDTO createDto);
        (int statuscode, string message, int EraId) Update(EraEditDTO editDto);
        (int statusCode, string message) Delete(int id);
    }
    public class EraProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITrackEventsText trackEvents) : IProcessEra
    {
        public (int statuscode, string message, int EraId) Insert(EraCreateDTO createDTO)
        {
            int? eraID = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(Era)],
                TranslatedText = [createDTO.Name]
            }).Select(x => x.EntityId).FirstOrDefault();
            if (eraID > 0)
            {
                trackEvents.TrackError("EraProcessor.Create: Era already exists.", new Dictionary<string, object>
                {
                    { "Era",createDTO}
                });
                return (303, "Error_Era_Exists", (int)eraID);
            }

            try
            {
                using TransactionScope transactionScope = new();

                Era newEra = new()
                {
                    WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                };
                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();

                processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Name,
                        EntityName = nameof(Era),
                        EntityId = newEra.EraID,
                        PropertyName = nameof(EraDisplayDTO.EraName)
                    });

                transactionScope.Complete();
                return (201, "Success_Era_Created", newEra.EraID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "EraProcessor.Create: Exception occurred while creating Era.", new Dictionary<string, object>
                {
                    { "Era", createDTO}
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public (int statuscode, string message, int EraId) Update(EraEditDTO edit)
        {
            Era? existingEra = GetEntityListViaPredicates(new EraSearchParameterModel { EraID = [edit.Id] }).FirstOrDefault();
            if (existingEra == null)
            {
                trackEvents.TrackError("EraProcessor.Edit: Era not found.", new Dictionary<string, object>
                {
                    { "Era", edit}
                });
                return (404, "Error_Era_NotFound", 0);
            }

            try
            {
                using TransactionScope transactionScope = new();
                bool isChanged = false;

                var translation = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    TranslatedText = [edit.Name],
                    EntityName = [nameof(Era)],
                    PropertyName = [nameof(EraDisplayDTO.EraName)]
                }).First();
                if (translation.EntityId != existingEra.EraID)
                {
                    processTranslations.Update(
                        new TranslationDTO
                        {
                            TextToTranslate = edit.Name,
                            EntityName = nameof(Era),
                            EntityId = existingEra.EraID,
                            PropertyName = nameof(EraDisplayDTO.EraName)
                        });
                    isChanged = true;
                }
                string? wikipediaUrlWithoutSubdomain = edit.WikipediaUrl.ChangeStringToUriToRemoveSubdomain();
                if (existingEra.WikipediaUrl != wikipediaUrlWithoutSubdomain)
                {
                    existingEra.WikipediaUrl = wikipediaUrlWithoutSubdomain;
                    isChanged = true;
                }
                if (isChanged)
                {
                    unitOfWork.Save();
                }

                transactionScope.Complete();
                return (200, "Success_Era_Updated", existingEra.EraID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "EraProcessor.Edit: Exception occurred while editing Era.", new Dictionary<string, object>
                {
                    { "Era", edit}
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public List<Era> GetEntityListViaPredicates(EraSearchParameterModel eraSearchParameter)
        {
            List<Era> eraList = [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))];
            return eraList;
        }
        public List<EraDisplayDTO> GetTranslationsListViaPredicates(EraSearchParameterModel eraSearchParameter)
        {
            List<EraDisplayDTO> eraList = [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))
                .AsNoTracking()
                .Select(e => new EraDisplayDTO
                {
                    EraID = e.EraID,
                    WikipediaUrl = e.WikipediaUrl
                })];
            SetTranslations(eraList);

            return [.. eraList.OrderBy(x => x.EraName)];
        }

        private void SetTranslations(List<EraDisplayDTO> eraList)
        {
            var allTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(Era)],
                PropertyName = [nameof(EraDisplayDTO.EraName)]
            }).ToList();
            foreach (EraDisplayDTO era in eraList)
            {
                era.EraName = allTranslations.FirstOrDefault(t => t.EntityId == era.EraID)?.TranslatedText ?? string.Empty;
            }
        }

        public (int statusCode, string message) Delete(int id)
        {
            Era? era = GetEntityListViaPredicates(new EraSearchParameterModel { EraID = [id] }).FirstOrDefault();
            if (era == null)
            {
                trackEvents.TrackError("EraProcessor.Delete: Era not found.", new Dictionary<string, object>
                {
                    { "EraId", id}
                });
                return (404, "Error_Era_NotFound");
            }
            if (era.CollectionItemEntityList.Count > 1)
            {
                trackEvents.TrackError("EraProcessor.Delete: Era cannot be deleted because it is associated with collection items.", new Dictionary<string, object>
                {
                    { "EraId", id},
                    { "AssociatedCollectionItemsCount", era.CollectionItemEntityList.Count}
                });
                return (400, "Error_Era_AssociatedWithCollectionItems");
            }

            try
            {
                using TransactionScope transactionScope = new();

                processTranslations.Delete(new EntityTranslationSearchParameter
                {
                    EntityName = [nameof(Era)],
                    EntityId = [id]
                });

                unitOfWork.EraRepository.Delete(era);
                unitOfWork.Save();

                transactionScope.Complete();
                return (200, "Success_Era_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "EraProcessor.Delete: Exception occurred while deleting Era.", new Dictionary<string, object>
                {
                    { "EraId", id}
                });
                return (500, "Error_Unknown");
            }
        }
    }
}
