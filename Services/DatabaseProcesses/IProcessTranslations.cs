using LinqKit;
using NuGet.Packaging;
using Sammlerplattform.Data;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using static System.Net.Mime.MediaTypeNames;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessTranslations
    {
        Dictionary<string,string> GetTranslationDictionary(
            string entityType,
            int entityId);
        List<EntityTranslation> GetWithPredicate(EntityTranslationSearchParameter searchParameter);
        List<string> Insert(EntityTranslation entityTranslation, string textToTranslate);
        List<string> Update(EntityTranslation entityTranslation, string textToTranslate);
        void Delete(EntityTranslationSearchParameter searchParameter);
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

        public List<string> Insert(EntityTranslation entityTranslation, string textToTranslate)
        {
            if(string.IsNullOrWhiteSpace(textToTranslate))
                return [];

            //Damit weniger Traffic entsteht, wird der Originaltext direkt als Originalsprache gespeichert
            List<(string Culture, string TranslatedText, string? Abbreviation)> translationList = entityTranslation.Culture != null
                ? new() { (entityTranslation.Culture, textToTranslate, entityTranslation.Abbreviation) }
                : [];
            // Alle anderen Sprachen werden via Deepl übersetzt
            translationList.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(textToTranslate, entityTranslation.Culture ).Result);

            List<string> translatedTexts = [];
            foreach (var (Culture, TranslatedText, Abbreviation) in translationList)
            {
                var newEntityTranslation = new EntityTranslation
                {
                    EntityType = entityTranslation.EntityType,
                    EntityId = entityTranslation.EntityId,
                    FieldName = entityTranslation.FieldName,
                    Culture = Culture,
                    TranslatedText = TranslatedText,
                    Abbreviation = Abbreviation
                };
                unitOfWork.EntityTranslationRepository.Insert(newEntityTranslation);

                translatedTexts.Add(TranslatedText);
                if(!string.IsNullOrEmpty(Abbreviation))
                    translatedTexts.Add(Abbreviation);
            }
            unitOfWork.Save();

            return translatedTexts;
        }
        public List<string> Update(EntityTranslation entityTranslation, string textToTranslate)
        {
            if (string.IsNullOrWhiteSpace(textToTranslate))
                return [];

            List<EntityTranslation> existingTranslations = [.. unitOfWork.EntityTranslationRepository.Get(
                filter: et => et.EntityType == entityTranslation.EntityType &&
                              et.EntityId == entityTranslation.EntityId &&
                              et.FieldName == entityTranslation.FieldName)];
            if (existingTranslations.Count == 0)
            { 
                throw new Exception("No existing translations found to update.");
            }

            List<string> translatedTexts = [];
            bool hasChanges = false;
            if (existingTranslations.Where(x => x.Culture == entityTranslation.Culture).FirstOrDefault()?.TranslatedText != textToTranslate)
            {
                List<(string Culture, string TranslatedText, string? Abbreviation)> translationList = entityTranslation.Culture != null
                        ? new() { ( entityTranslation.Culture, textToTranslate, entityTranslation.Abbreviation ) }
                        : [];
                translationList.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(textToTranslate, entityTranslation.Culture).Result);

                foreach (var existingTranslation in existingTranslations)
                {
                    existingTranslation.TranslatedText = translationList.FirstOrDefault(x => x.Culture == existingTranslation.Culture).TranslatedText;
                    translatedTexts.Add(existingTranslation.TranslatedText);
                }
                if(existingTranslations.Count != translationList.Count)
                {
                    // Neue Übersetzungen hinzufügen, die noch nicht existieren
                    var existingCultures = existingTranslations.Select(et => et.Culture).ToHashSet();
                    var newTranslations = translationList
                        .Where(t => !existingCultures.Contains(t.Culture))
                        .Select(t => new EntityTranslation
                        {
                            EntityType = entityTranslation.EntityType,
                            EntityId = entityTranslation.EntityId,
                            FieldName = entityTranslation.FieldName,
                            Culture = t.Culture,
                            TranslatedText = t.TranslatedText,
                            Abbreviation = t.Abbreviation
                        }
                        ).ToList();
                    translatedTexts.AddRange(newTranslations.Select(nt => nt.TranslatedText));
                    translatedTexts.AddRange(newTranslations.Where(nt => !string.IsNullOrEmpty(nt.Abbreviation)).Select(nt => nt.Abbreviation!));
                    foreach (var newTranslation in newTranslations)
                    {
                        unitOfWork.EntityTranslationRepository.Insert(newTranslation);
                    }
                }
                hasChanges = true;
            }
            if(existingTranslations.Where(x => x.Culture == entityTranslation.Culture).FirstOrDefault()?.Abbreviation != entityTranslation.Abbreviation)
            {
                existingTranslations.Where(x => x.Culture == entityTranslation.Culture).FirstOrDefault()!.Abbreviation = entityTranslation.Abbreviation;
                translatedTexts.Add(entityTranslation.Abbreviation!);
                hasChanges = true;    
            }
            if (hasChanges)
            {
                unitOfWork.Save();
            }

            return translatedTexts;
        }
        public void Delete(EntityTranslationSearchParameter searchParameter)
        {
            var translation = GetWithPredicate(searchParameter).FirstOrDefault();
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
