using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Extensions;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessEra
    {
        List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter);
        (int statuscode, string message, int EraId) Insert(EraCreateDTO createDto);
        (int statuscode, string message, int EraId) Update(EraEditDTO editDto);
        (int statusCode, string message) Delete(int id);
    }
    public class EraProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents) : IProcessEra
    {
        public (int statuscode, string message, int EraId) Insert(EraCreateDTO createDTO)
        {
            int? eraID = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(Era)],
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
                    EraName = createDTO.Name,
                    WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                };
                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();

                processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Name,
                        EntityType = nameof(Era),
                        EntityId = newEra.EraID,
                        FieldName = nameof(Era.EraName),
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
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
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int statuscode, string message, int EraId) Update(EraEditDTO edit)
        {
            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraID == edit.Id).FirstOrDefault();
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
                if (existingEra.EraName != edit.Name)
                {
                    processTranslations.Update(
                        new TranslationDTO
                        {
                            TextToTranslate = edit.Name,
                            EntityType = nameof(Era),
                            EntityId = existingEra.EraID,
                            FieldName = nameof(Era.EraName),
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
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
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter)
        {
            List<Era> eraList = [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))];

            foreach (Era era in eraList)
            {
                era.EraName = translationStore.GetTranslation(
                    nameof(Era),
                    era.EraID,
                    nameof(era.EraName),
                    translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name))
                    ?? "";
            }

            return [.. eraList.OrderBy(x => x.EraName)];
        }

        public (int statusCode, string message) Delete(int id)
        {
            Era? era = GetWithPredicates(new EraSearchParameterModel { EraID = [id] }).FirstOrDefault();
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
                    EntityType = [nameof(Era)],
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
                return (500, "Error_Error_Ocurred");
            }
        }
    }
}
