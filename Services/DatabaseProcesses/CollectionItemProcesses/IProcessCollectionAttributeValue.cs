using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionAttributeValue
    {
        (int Statuscode, string Statusmessage, List<string>) Insert(CollectionAttributeValue collectionAttributeValue, int collectionItemID);
        (int Statuscode, string Statusmessage, List<string>) Update(CollectionAttributeValue collectionAttributeValue);
        (int Statuscode, string Statusmessage) Delete(int collectionAttributeValueID);
    }

    public class CollectionAttributeValueProcessor(IUnitOfWork unitOfWork,
        DeeplTranslationService translationService,
        IProcessTranslations processTranslations) : IProcessCollectionAttributeValue
    {
        public (int Statuscode, string Statusmessage, List<string>) Insert(CollectionAttributeValue collectionAttributeValue, int collectionItemEntityID)
        {
            if (collectionItemEntityID <= 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing", []);
            }
           
            collectionAttributeValue.CollectionItemEntityID = collectionItemEntityID;
            if(!string.IsNullOrWhiteSpace(collectionAttributeValue.ValueString))
            {
                collectionAttributeValue.ValueString = translationService.SetIntoFallbackLanguage(collectionAttributeValue.ValueString);
            }            
            _ = unitOfWork.CollectionAttributeValueRepository.Insert(collectionAttributeValue);
            unitOfWork.Save();

            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(collectionAttributeValue.ValueString))
            {
                translationList = [.. processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionAttributeValue),
                        EntityId = collectionAttributeValue.CollectionAttributeValueID,
                        FieldName = nameof(CollectionAttributeValue.ValueString),
                        TranslatedText = collectionAttributeValue.ValueString,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionAttributeValue.ValueString).Values];
            }

            return (200, "Success_CollectionAttributeValue_Created", translationList);
        }

        public (int Statuscode, string Statusmessage, List<string>) Update(CollectionAttributeValue collectionAttributeValue)
        {
            CollectionAttributeValue? existingCollectionAttributeValue = unitOfWork.CollectionAttributeValueRepository.Get(ci =>
                ci.CollectionAttributeValueID == collectionAttributeValue.CollectionAttributeValueID).FirstOrDefault();
            if (existingCollectionAttributeValue == null)
            {
                return (404, "Error_CollectionAttributeValue_NotFound", []);
            }

            bool hasChanges = false;
            List<string> translationList = [];
            if (!string.IsNullOrWhiteSpace(collectionAttributeValue.ValueString))
            {
                collectionAttributeValue.ValueString = translationService.SetIntoFallbackLanguage(collectionAttributeValue.ValueString);

                if (existingCollectionAttributeValue.ValueString != collectionAttributeValue.ValueString)
                {
                    existingCollectionAttributeValue.ValueString = collectionAttributeValue.ValueString;
                    translationList = [.. processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionAttributeValue),
                            EntityId = existingCollectionAttributeValue.CollectionAttributeValueID,
                            FieldName = nameof(CollectionAttributeValue.ValueString),
                            TranslatedText = collectionAttributeValue.ValueString,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        collectionAttributeValue.ValueString).Values];
                    hasChanges = true;
                }
            }
            if (existingCollectionAttributeValue.ValueInt != collectionAttributeValue.ValueInt)
            {
                existingCollectionAttributeValue.ValueInt = collectionAttributeValue.ValueInt;
                hasChanges = true;
            }
            if (existingCollectionAttributeValue.ValueDate != collectionAttributeValue.ValueDate)
            {
                existingCollectionAttributeValue.ValueDate = collectionAttributeValue.ValueDate;
                hasChanges = true;
            }
            if (existingCollectionAttributeValue.ValueDecimal != collectionAttributeValue.ValueDecimal)
            {
                existingCollectionAttributeValue.ValueDecimal = collectionAttributeValue.ValueDecimal;
                hasChanges = true;
            }
            if (existingCollectionAttributeValue.ValueBool != collectionAttributeValue.ValueBool)
            {
                existingCollectionAttributeValue.ValueBool = collectionAttributeValue.ValueBool;
                hasChanges = true;
            }
            if (hasChanges)
                unitOfWork.Save();

            return (200, "Success_CollectionAttributeValue_Updated", translationList);
        }

        public (int Statuscode, string Statusmessage) Delete(int collectionAttributeValueID)
        {
            if (collectionAttributeValueID <= 0)
            {
                return (400, "Error_CollectionAttributeValue_IDMissing");
            }

            CollectionAttributeValue? existingCollectionAttributeValue = unitOfWork.CollectionAttributeValueRepository.Get(ci =>
                ci.CollectionAttributeValueID == collectionAttributeValueID).FirstOrDefault();
            if (existingCollectionAttributeValue == null)
            {
                return (404, "Error_CollectionAttributeValue_NotFound");
            }
            unitOfWork.CollectionAttributeValueRepository.Delete(existingCollectionAttributeValue);
            unitOfWork.Save();

            return (200, "Success_CollectionAttributeValue_Deleted");
        }
    }
}
