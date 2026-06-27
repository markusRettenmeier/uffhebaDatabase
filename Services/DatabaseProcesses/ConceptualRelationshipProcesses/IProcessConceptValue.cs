using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.Translations;

namespace Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses
{
    public interface IProcessConceptValue
    {
        List<string> Insert(ConceptValue conceptValue, int collectionItemID);
        List<string> Update(ConceptValue conceptValue);
        List<ConceptValue> Get(ConceptValueSearchParameterModel searchParameters);
        void Delete(int conceptValueID);
    }

    public class ConceptValueProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations) : IProcessConceptValue
    {
        public List<string> Insert(ConceptValue conceptValue, int collectionItemEntityID)
        {
            conceptValue.CollectionItemEntityID = collectionItemEntityID;
            _ = unitOfWork.ConceptValueRepository.Insert(conceptValue);
            unitOfWork.Save();

            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(conceptValue.ValueString))
            {
                translationList = processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = conceptValue.ValueString,
                        EntityName = nameof(ConceptValue),
                        EntityId = conceptValue.ConceptValueID,
                        PropertyName = nameof(ConceptValue.ValueString)
                    });
            }

            return translationList;
        }

        public List<string> Update(ConceptValue conceptValue)
        {
            ConceptValue? existingConceptValue = unitOfWork.ConceptValueRepository.Get(ci =>
                ci.ConceptValueID == conceptValue.ConceptValueID).First();

            bool hasChanges = false;
            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(conceptValue.ValueString))
            {
                if (existingConceptValue.ValueString != conceptValue.ValueString)
                {
                    translationList = processTranslations.Update(
                        new TranslationDTO
                        {
                            TextToTranslate = conceptValue.ValueString,
                            EntityName = nameof(ConceptValue),
                            EntityId = existingConceptValue.ConceptValueID,
                            PropertyName = nameof(ConceptValue.ValueString)
                        });
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

            return translationList;
        }

        public void Delete(int conceptValueID)
        {
            ConceptValue existingConceptValue = unitOfWork.ConceptValueRepository.Get(ci =>
                ci.ConceptValueID == conceptValueID).First();
            unitOfWork.ConceptValueRepository.Delete(existingConceptValue);
            unitOfWork.Save();;
        }

        public List<ConceptValue> Get(ConceptValueSearchParameterModel searchParameters)
        {
            List<ConceptValue> conceptValueList = [.. unitOfWork.ConceptValueRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<ConceptValue>(searchParameters))];
            return conceptValueList;
        }
    }
}
