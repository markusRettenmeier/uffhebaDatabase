using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessEra
    {
        List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter);
        (int statuscode, string message, int EraId) Insert(Era era);
        (int statuscode, string message, int EraId) Update(Era era);
    }
    public class EraProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEvents trackEvents) : IProcessEra
    {
        public (int statuscode, string message, int EraId) Insert(Era era)
        {
            if (string.IsNullOrEmpty(era.EraName))
            {
                trackEvents.TrackWarning("EraProcessor.Create: EraName is missing.", new Dictionary<string, object>
                {
                    { "Era", era}
                });
                return (404, "Error_EraName_Missing", 0);
            }

            EraSearchParameterModel searchParameterModel = new() 
            {
                EraID = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(Era)],
                    TranslatedText = [era.EraName]
                }).Select(x => x.EntityId).Distinct()]
            };
            if(searchParameterModel.EraID.Count > 0)
            {
                trackEvents.TrackWarning("EraProcessor.Create: Era already exists.", new Dictionary<string, object>
                {
                    { "Era",era}
                });
                return (303, "Error_Era_Exists", era.EraID);
            }
            
            try
            {
                TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                Era newEra = unitOfWork.EraRepository.Insert(era);
                unitOfWork.Save();

                processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(Era),
                        EntityId = newEra.EraID,
                        FieldName = nameof(era.EraName),
                        TranslatedText = era.EraName,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    era.EraName);

                transactionScope.Complete();
                return (201, "Success_Era_Created", newEra.EraID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "EraProcessor.Create: Exception occurred while creating Era.", new Dictionary<string, object>
                {
                    { "Era", era}
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int statuscode, string message, int EraId) Update(Era era)
        {
            if (era.EraID <= 0)
            {
                trackEvents.TrackWarning("EraProcessor.Edit: EraID is missing or invalid.", new Dictionary<string, object>
                {
                    { "Era", era}
                });
                return (404, "Error_Era_IdMissing", 0);
            }
            if (string.IsNullOrEmpty(era.EraName))
            {
                trackEvents.TrackWarning("EraProcessor.Edit: EraName is missing.", new Dictionary<string, object>
                {
                    { "Era", era}
                });
                return (404, "Error_EraName_Missing", 0);
            }

            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraID == era.EraID).FirstOrDefault();
            if (existingEra == null)
            {
                trackEvents.TrackWarning("EraProcessor.Edit: Era not found.", new Dictionary<string, object>
                {
                    { "Era", era}
                });
                return (404, "Error_Era_NotFound", 0);
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                bool isChanged = false;
                if(existingEra.StartYear != era.StartYear)
                {
                    existingEra.StartYear = era.StartYear;
                    isChanged = true;
                }
                if(existingEra.EndYear != era.EndYear)
                {
                    existingEra.EndYear = era.EndYear;
                    isChanged = true;
                }
                if(existingEra.EraName != era.EraName)
                {
                    processTranslations.Update(
                        new EntityTranslation
                        {
                            EntityType = nameof(Era),
                            EntityId = existingEra.EraID,
                            FieldName = nameof(era.EraName),
                            TranslatedText = era.EraName,
                            Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                        },
                        era.EraName);
                    isChanged = true;
                }
                if(existingEra.WikipediaUrl != era.WikipediaUrl)
                {
                    existingEra.WikipediaUrl = era.WikipediaUrl;
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
                    { "Era", era}
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
                    ?? era.EraName;
            }

            return [.. eraList.OrderBy(x => x.EraName)];
        }
    }
}
