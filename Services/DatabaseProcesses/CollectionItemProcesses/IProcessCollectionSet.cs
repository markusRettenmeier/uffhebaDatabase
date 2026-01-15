using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionSet
    {
        (int StatusCode, string StatusMessage, int SetID) Insert(CollectionSet set);
        (int StatusCode, string StatusMessage, int SetID) Update(CollectionSet set);
        (int StatusCode, string StatusMessage) Delete(CollectionSet set);
        List<CollectionSet> GetWithPredicates(CollectionSetSearchParameterModel searchParameter);
    }

    public class CollectionSetProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        IDeeplTranslationService translationService,
        ITrackEvents trackEvents) : IProcessCollectionSet
    {
        public (int StatusCode, string StatusMessage, int SetID) Insert(CollectionSet set)
        {
            if (string.IsNullOrEmpty(set.CollectionSetName))
            {
                trackEvents.TrackWarning("CollectionSetProcessor/Insert: CollectionSetName is missing.",new Dictionary<string, object>
                {
                    {"CollectionSet", set }
                });
                return (400, "Error_CollectionSet_NameMissing", 0);
            };

            try
            {
                TransactionScope scope = new();

                CollectionSet newSet = unitOfWork.SetRepository.Insert(set);
                unitOfWork.Save();

                processTranslations.Insert(
                    new Models.Translations.EntityTranslation
                    {
                        EntityType = nameof(CollectionSet),
                        EntityId = newSet.CollectionSetId,
                        FieldName = nameof(CollectionSet.CollectionSetName),
                        TranslatedText = set.CollectionSetName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    set.CollectionSetName);

                scope.Complete();
                return (200, "Success_CollectionSet_Created", newSet.CollectionSetId);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "CollectionSetProcessor/Insert", new Dictionary<string, object>
                {
                    { "CollectionSet", set }
                });
                return (500, "Error_CollectionSet_CreationFailed", 0);
            }
        }

        public (int StatusCode, string StatusMessage) Delete(CollectionSet set)
        {
            try
            {
                unitOfWork.SetRepository.Delete(set);
                unitOfWork.Save();

                return (200, "Success_CollectionSet_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "CollectionSetProcessor/Delete", new Dictionary<string, object>
                {
                    { "CollectionSet", set }
                });
                return (500, "Error_CollectionSet_DeletionFailed");
            }
        }

        public List<CollectionSet> GetWithPredicates(CollectionSetSearchParameterModel searchParameter)
        {
            IEnumerable<CollectionSet> query = unitOfWork.SetRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionSet>(searchParameter),
                includeProperties: nameof(CollectionSet.CollectionItemEntityList) + "." + nameof(CollectionItemEntity.CollectionItemPictureList));

            foreach (CollectionSet set in query)
            {
                var translation = translationStore.GetTranslation(
                    nameof(CollectionSet),
                    set.CollectionSetId,
                    nameof(CollectionSet.CollectionSetName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name));
                if (translation != null)
                {
                    set.CollectionSetName = translation;
                }
            }

            return [.. query.OrderBy(x => x.CollectionSetName)];
        }

        public (int StatusCode, string StatusMessage, int SetID) Update(CollectionSet set)
        {
            if (set.CollectionSetId <= 0)
            {
                return (400, "Error_CollectionSet_IdMissing", 0);
            }
            if (string.IsNullOrEmpty(set.CollectionSetName))
            {
                return (400, "Error_CollectionSet_NameMissing", 0);
            }

            CollectionSet? existingSet = unitOfWork.SetRepository.GetByID(set.CollectionSetId);
            if (existingSet == null)
            {
                return (400, "Error_CollectionSet_NotFound", 0);
            }

            try
            {
                TransactionScope scope = new();
                var existingTranslations = processTranslations.GetWithPredicate(new Models.Translations.EntityTranslationSearchParameter
                {
                    EntityType = [nameof(CollectionSet)],
                    EntityId = [set.CollectionSetId],
                    FieldName = [nameof(CollectionSet.CollectionSetName)],
                    Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                }).FirstOrDefault();
                if (existingTranslations != null && existingTranslations.TranslatedText != set.CollectionSetName)
                {
                    processTranslations.Update(
                        new Models.Translations.EntityTranslation
                        {
                            EntityType = nameof(CollectionSet),
                            EntityId = set.CollectionSetId,
                            FieldName = nameof(CollectionSet.CollectionSetName),
                            TranslatedText = set.CollectionSetName,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        set.CollectionSetName);
                }

                scope.Complete();
                return (200, "Success_CollectionSet_Updated", set.CollectionSetId);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "CollectionSetProcessor/Update", new Dictionary<string, object>
                {
                    { "CollectionSet", set }
                });
                return (500, "Error_CollectionSet_UpdateFailed", 0);
            }
        }
    }
}
