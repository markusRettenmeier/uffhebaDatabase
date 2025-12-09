using LinqKit;
using NuGet.Packaging;
using Sammlerplattform.Data;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessTranslations
    {
        Dictionary<string,string> GetTranslationDictionary(
            string entityType,
            int entityId);
        List<EntityTranslation> GetWithPredicate(EntityTranslationSearchParameter searchParameter);
        Dictionary<string, string> Create(EntityTranslation entityTranslation, string textToTranslate);
        Dictionary<string, string> Edit(EntityTranslation entityTranslation, string textToTranslate);
        void Delete(
            string entityType,
            int entityId,
            string field,
            string culture);
    }
    public class ProcessTranslations(IUnitOfWork unitOfWork, IDeeplTranslationService deeplTranslationService) : IProcessTranslations
    {
        public Dictionary<string, string> GetTranslationDictionary(
            string entityType,
            int entityId)
        {
            var translations = unitOfWork.EntityTranslationRepository.Get(
                filter: et => et.EntityType == entityType && et.EntityId == entityId);
            return translations.ToDictionary(
                t => $"{t.FieldName}:{t.Culture}",
                t => t.TranslatedText);
        }

        public Dictionary<string, string> Create(EntityTranslation entityTranslation, string textToTranslate)
        {
            if(string.IsNullOrWhiteSpace(textToTranslate))
                return [];

            //Damit weniger traffic entsteht, wird der Originaltext auch als Übersetzung in der Originalsprache gespeichert
            Dictionary<string, string> dictionaryWithTranslations = entityTranslation.Culture != null
                ? new Dictionary<string, string> { { entityTranslation.Culture, textToTranslate } }
                : [];
            dictionaryWithTranslations.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(textToTranslate, entityTranslation.Culture).Result);

            foreach (var translation in dictionaryWithTranslations)
            {
                var newEntityTranslation = new EntityTranslation
                {
                    EntityType = entityTranslation.EntityType,
                    EntityId = entityTranslation.EntityId,
                    FieldName = entityTranslation.FieldName,
                    Culture = translation.Key,
                    TranslatedText = translation.Value
                };
                unitOfWork.EntityTranslationRepository.Insert(newEntityTranslation);
            }
            unitOfWork.Save();

            return dictionaryWithTranslations;
        }
        public Dictionary<string, string> Edit(EntityTranslation entityTranslation, string textToTranslate)
        {
            if (string.IsNullOrWhiteSpace(textToTranslate))
                return [];

            List<EntityTranslation> existingTranslations = [.. unitOfWork.EntityTranslationRepository.Get(
                filter: et => et.EntityType == entityTranslation.EntityType &&
                              et.EntityId == entityTranslation.EntityId &&
                              et.FieldName == entityTranslation.FieldName)];
            if(existingTranslations.Where(x => x.Culture == entityTranslation.Culture).FirstOrDefault()?.TranslatedText != textToTranslate)
            {
                Dictionary<string, string> dictionaryWithTranslations = entityTranslation.Culture != null
                ? new Dictionary<string, string> { { entityTranslation.Culture, textToTranslate } }
                : [];
                dictionaryWithTranslations.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(textToTranslate, entityTranslation.Culture).Result);
                //var dictionaryWithTranslations = deeplTranslationService.TranslateTextDifferentCulturesAsync(textToTranslate).Result;

                foreach (var existingTranslation in existingTranslations)
                {
                    existingTranslation.TranslatedText = dictionaryWithTranslations.ContainsKey(existingTranslation.Culture)
                        ? dictionaryWithTranslations[existingTranslation.Culture]
                        : existingTranslation.TranslatedText;
                }
                unitOfWork.Save();
            }

            return existingTranslations.ToDictionary(
                t => t.Culture,
                t => t.TranslatedText);
        }
        public void Delete(
            string entityType,
            int entityId,
            string field,
            string culture)
        {
            var translation = unitOfWork.EntityTranslationRepository.Get(
                filter: et => et.EntityType == entityType &&
                              et.EntityId == entityId &&
                              et.FieldName == field &&
                              et.Culture == culture).FirstOrDefault();
            if (translation != null)
            {
                unitOfWork.EntityTranslationRepository.Delete(translation);
                unitOfWork.Save();
            }
        }

        public List<EntityTranslation> GetWithPredicate(EntityTranslationSearchParameter searchParameter)
        {
            IEnumerable<EntityTranslation> query = unitOfWork.EntityTranslationRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<EntityTranslation>(searchParameter));

            return [..query];
        }
    }
}
