using Sammlerplattform.Data;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessTranslations
    {
        List<EntityTranslation> GetWithFallback(EntityTranslationSearchParameter searchParameter);
        List<string> Insert(TranslationDTO createDTO);
        List<string> Update(TranslationDTO editDTO);
        void Delete(EntityTranslationSearchParameter searchParameter);
    }
    public class ProcessTranslations(IUnitOfWork unitOfWork
        , IDeeplTranslationService deeplTranslationService) : IProcessTranslations
    {
        public List<string> Insert(TranslationDTO createDTO)
        {            
            if (string.IsNullOrWhiteSpace(createDTO.TextToTranslate))
                return [];
            string culture = deeplTranslationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name);

            List<string> translatedTexts = [];
            if (createDTO.IsTranslateable)
            {
                //Damit weniger Traffic entsteht, wird der Originaltext direkt als Originalsprache gespeichert
                List<(string Culture, string TranslatedText, string? Abbreviation)> translationList = culture != null
                    ? new() { (culture, createDTO.TextToTranslate, createDTO.Abbreviation) }
                    : [];
                // Alle anderen Sprachen werden via Deepl übersetzt
                translationList.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(createDTO.TextToTranslate, culture).Result);

                foreach (var (Culture, TranslatedText, Abbreviation) in translationList)
                {
                    var newEntityTranslation = new EntityTranslation
                    {
                        EntityName = createDTO.EntityName,
                        EntityId = createDTO.EntityId,
                        PropertyName = createDTO.PropertyName,
                        Culture = Culture,
                        TranslatedText = TranslatedText,
                        Abbreviation = Abbreviation
                    };
                    unitOfWork.EntityTranslationRepository.Insert(newEntityTranslation);

                    translatedTexts.Add(TranslatedText);
                    if (!string.IsNullOrEmpty(Abbreviation))
                        translatedTexts.Add(Abbreviation);
                }
            }
            else
            {
                var newEntityTranslation = new EntityTranslation
                {
                    EntityName = createDTO.EntityName,
                    EntityId = createDTO.EntityId,
                    PropertyName = createDTO.PropertyName,
                    TranslatedText = createDTO.TextToTranslate,
                    Abbreviation = createDTO.Abbreviation,
                    Culture = "iv" // invariant
                };
                unitOfWork.EntityTranslationRepository.Insert(newEntityTranslation);

                translatedTexts.Add(createDTO.TextToTranslate);
            }

            unitOfWork.Save();

            return translatedTexts;
        }
        public List<string> Update(TranslationDTO editDTO)
        {
            if (string.IsNullOrWhiteSpace(editDTO.TextToTranslate))
                return []; 
            string culture = deeplTranslationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name);

            List<EntityTranslation> existingTranslations = GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [editDTO.EntityName],
                PropertyName = [editDTO.PropertyName],
                EntityId = [editDTO.EntityId]
            });
            if (existingTranslations.Count == 0)
            {
                throw new Exception("No existing translations found to update.");
            }

            List<string> translatedTexts = [];
            bool hasChanges = false;

            var target = existingTranslations.FirstOrDefault(x => x.Culture == culture);
            if (target == null)
            {
                return [];
            }

            // 🔹 1. TEXT UPDATE
            if (target.TranslatedText != editDTO.TextToTranslate)
            {
                target.TranslatedText = editDTO.TextToTranslate;
                hasChanges = true;

                // 👉 Nur übersetzen wenn erlaubt
                if (editDTO.IsTranslateable)
                {
                    var translationList =
                        deeplTranslationService
                            .TranslateTextDifferentCulturesAsync(editDTO.TextToTranslate, culture).Result;

                    // Dictionary für schnellen Zugriff
                    var translationDict = translationList
                        .ToDictionary(t => t.culture, t => t);

                    foreach (var translation in existingTranslations)
                    {
                        if (translation.Culture == culture)
                            continue;

                        if (translationDict.TryGetValue(translation.Culture, out var newTranslation))
                        {
                            translation.TranslatedText = newTranslation.translatedText;
                            translation.Abbreviation = newTranslation.abbreviation;
                        }
                        // 👉 Fallback NICHT überschreiben!
                    }
                }
                else
                {
                    // 👉 invariant: alle gleich setzen
                    foreach (var translation in existingTranslations)
                    {
                        translation.TranslatedText = editDTO.TextToTranslate;
                    }
                }
            }

            // 🔹 2. ABBREVIATION UPDATE (nur Zielkultur!)
            if (target.Abbreviation != editDTO.Abbreviation)
            {
                target.Abbreviation = editDTO.Abbreviation;
                hasChanges = true;
            }
            // 🔹 3. SAVE
            if (hasChanges)
            {
                unitOfWork.Save();
            }

            return translatedTexts;
        }
        public void Delete(EntityTranslationSearchParameter searchParameter)
        {
            var translationList = GetWithFallback(searchParameter);
            if (translationList.Count > 0)
            {
                foreach (var translation in translationList)
                {
                    unitOfWork.EntityTranslationRepository.Delete(translation);
                }
                unitOfWork.Save();
            }
        }

        public List<EntityTranslation> GetWithFallback(EntityTranslationSearchParameter searchParameter)
        {
            string culture = deeplTranslationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name);
            searchParameter.Culture = [culture];
            List<EntityTranslation> result = [.. unitOfWork.EntityTranslationRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<EntityTranslation>(searchParameter)
            )];
            if (result.Count != 0)
                return result;

            // 2. Fallback: invariant
            searchParameter.Culture = ["iv"];
            result = [.. unitOfWork.EntityTranslationRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<EntityTranslation>(searchParameter)
            )];
            if (result.Count != 0)
                return result;

            // 3. Fallback: Englisch
            searchParameter.Culture = ["en"];
            return [.. unitOfWork.EntityTranslationRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<EntityTranslation>(searchParameter)
            )];
        }
    }
}
