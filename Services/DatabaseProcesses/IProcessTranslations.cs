using Sammlerplattform.Data;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessTranslations
    {
        Dictionary<string, string> GetTranslationDictionary(
            string entityType,
            int entityId);
        List<EntityTranslation> GetWithFallback(EntityTranslationSearchParameter searchParameter);
        List<string> Insert(TranslationDTO createDTO);
        List<string> Update(TranslationDTO editDTO);
        void Delete(EntityTranslationSearchParameter searchParameter);
    }
    public class ProcessTranslations(IUnitOfWork unitOfWork
        , IDeeplTranslationService deeplTranslationService) : IProcessTranslations
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

        public List<string> Insert(TranslationDTO createDTO)
        {
            if (string.IsNullOrWhiteSpace(createDTO.TextToTranslate))
                return [];

            List<string> translatedTexts = [];
            if (createDTO.IsTranslateable)
            {
                //Damit weniger Traffic entsteht, wird der Originaltext direkt als Originalsprache gespeichert
                List<(string Culture, string TranslatedText, string? Abbreviation)> translationList = createDTO.Culture != null
                    ? new() { (createDTO.Culture, createDTO.TextToTranslate, createDTO.Abbreviation) }
                    : [];
                // Alle anderen Sprachen werden via Deepl übersetzt
                translationList.AddRange(deeplTranslationService.TranslateTextDifferentCulturesAsync(createDTO.TextToTranslate, createDTO.Culture).Result);

                foreach (var (Culture, TranslatedText, Abbreviation) in translationList)
                {
                    var newEntityTranslation = new EntityTranslation
                    {
                        EntityType = createDTO.EntityType,
                        EntityId = createDTO.EntityId,
                        FieldName = createDTO.FieldName,
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
                    EntityType = createDTO.EntityType,
                    EntityId = createDTO.EntityId,
                    FieldName = createDTO.FieldName,
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

            List<EntityTranslation> existingTranslations = GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityType = [editDTO.EntityType],
                FieldName = [editDTO.FieldName],
                EntityId = [editDTO.EntityId]
            });
            if (existingTranslations.Count == 0)
            {
                throw new Exception("No existing translations found to update.");
            }

            List<string> translatedTexts = [];
            bool hasChanges = false;

            var target = existingTranslations.FirstOrDefault(x => x.Culture == editDTO.Culture);
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
                            .TranslateTextDifferentCulturesAsync(editDTO.TextToTranslate, editDTO.Culture).Result;

                    // Dictionary für schnellen Zugriff
                    var translationDict = translationList
                        .ToDictionary(t => t.culture, t => t);

                    foreach (var translation in existingTranslations)
                    {
                        if (translation.Culture == editDTO.Culture)
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
            // 1. Primäre Sprache
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
