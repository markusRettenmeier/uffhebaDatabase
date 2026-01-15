using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses
{
    public interface IProcessConceptValue
    {
        (int Statuscode, string Statusmessage, List<string>) Insert(ConceptValue conceptValue, int collectionItemID);
        (int Statuscode, string Statusmessage, List<string>) Update(ConceptValue conceptValue);
        (int Statuscode, string Statusmessage) Delete(int conceptValueID);
    }

    public class ConceptValueProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations) : IProcessConceptValue
    {
        public (int Statuscode, string Statusmessage, List<string>) Insert(ConceptValue conceptValue, int collectionItemEntityID)
        {
            if (collectionItemEntityID <= 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing", []);
            }
           
            conceptValue.CollectionItemEntityID = collectionItemEntityID;           
            _ = unitOfWork.ConceptValueRepository.Insert(conceptValue);
            unitOfWork.Save();

            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(conceptValue.ValueString))
            {
                translationList = processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(ConceptValue),
                        EntityId = conceptValue.ConceptValueID,
                        FieldName = nameof(ConceptValue.ValueString),
                        TranslatedText = conceptValue.ValueString,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    conceptValue.ValueString);
            }

            return (200, "Success_ConceptValue_Created", translationList);
        }

        public (int Statuscode, string Statusmessage, List<string>) Update(ConceptValue conceptValue)
        {
            ConceptValue? existingConceptValue = unitOfWork.ConceptValueRepository.Get(ci =>
                ci.ConceptValueID == conceptValue.ConceptValueID).FirstOrDefault();
            if (existingConceptValue == null)
            {
                return (404, "Error_ConceptValue_NotFound", []);
            }

            bool hasChanges = false;
            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(conceptValue.ValueString))
            {
                if (existingConceptValue.ValueString != conceptValue.ValueString)
                {
                    translationList = processTranslations.Update(
                        new EntityTranslation
                        {
                            EntityType = nameof(ConceptValue),
                            EntityId = existingConceptValue.ConceptValueID,
                            FieldName = nameof(ConceptValue.ValueString),
                            TranslatedText = conceptValue.ValueString,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        conceptValue.ValueString);
                    hasChanges = true;
                }
            }
            if (existingConceptValue.ValueInt != conceptValue.ValueInt)
            {
                existingConceptValue.ValueInt = conceptValue.ValueInt;
                hasChanges = true;
            }
            if (existingConceptValue.ValueDate != conceptValue.ValueDate)
            {
                existingConceptValue.ValueDate = conceptValue.ValueDate;
                hasChanges = true;
            }
            if (existingConceptValue.ValueDecimal != conceptValue.ValueDecimal)
            {
                existingConceptValue.ValueDecimal = conceptValue.ValueDecimal;
                hasChanges = true;
            }
            if (existingConceptValue.ValueBool != conceptValue.ValueBool)
            {
                existingConceptValue.ValueBool = conceptValue.ValueBool;
                hasChanges = true;
            }
            if (hasChanges)
                unitOfWork.Save();

            return (200, "Success_ConceptValue_Updated", translationList);
        }

        public (int Statuscode, string Statusmessage) Delete(int conceptValueID)
        {
            if (conceptValueID <= 0)
            {
                return (400, "Error_ConceptValue_IdMissing");
            }

            ConceptValue? existingConceptValue = unitOfWork.ConceptValueRepository.Get(ci =>
                ci.ConceptValueID == conceptValueID).FirstOrDefault();
            if (existingConceptValue == null)
            {
                return (404, "Error_ConceeptValue_NotFound");
            }
            unitOfWork.ConceptValueRepository.Delete(existingConceptValue);
            unitOfWork.Save();

            return (200, "Success_ConceptValue_Deleted");
        }
    }
}
