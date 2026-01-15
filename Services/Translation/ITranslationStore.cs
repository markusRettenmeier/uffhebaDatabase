using Sammlerplattform.Services.DatabaseProcesses;
using System.Collections.Concurrent;

namespace Sammlerplattform.Services.Translation
{
    public interface ITranslationStore
    {
        string? GetTranslation(string entityType, int entityId, string field, string culture);
        List<string> GetById<T>(int entityId);
        int GetId<T>(string translatedText);
    }

    public class TranslationStore (IProcessTranslations processTranslations) : ITranslationStore
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache
            = new();

        public List<string> GetById<T>(int entityId)
        {
            var textInDictionary = processTranslations.GetTranslationDictionary(nameof(T), entityId);
            return [.. textInDictionary.Values];
        }

        public string? GetTranslation(
            string entityType,
            int entityId,
            string field,
            string culture)
        {
            var key = $"{entityType}:{entityId}:{field}:{culture}";

            if (!_cache.TryGetValue(key, out var entityCache))
            {
                // Aus DB laden und cachen
                entityCache = processTranslations.GetTranslationDictionary(entityType, entityId);
                _cache.TryAdd(key, entityCache);
            }

            return entityCache.TryGetValue($"{field}:{culture}", out var value)
                ? value
                : null;
        }  
        public int GetId<T>(string translatedText)
        {
            // Sucht in der DB nach der Übersetzung und gibt die zugehörige EntityId zurück
            var searchParameter = new Models.Translations.EntityTranslationSearchParameter
            {
                EntityType = [nameof(T)],
                TranslatedText = [translatedText]
            };
            var translations = processTranslations.GetWithPredicate(searchParameter);
            return translations.FirstOrDefault()?.EntityId ?? 0;
        }
    }
}
