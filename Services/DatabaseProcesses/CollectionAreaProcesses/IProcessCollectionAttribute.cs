using Microsoft.Build.Logging;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses
{
    public interface IProcessCollectionAttribute
    {
        List<CollectionAttribute> GetListWithPredicate(CollectionAttributeSearchParameterModel searchParameterModel);
        (int CollectionAreaID, int StatusCode, string StatusMessage) Create(CollectionAttribute collectionAttribute);
        (int CollectionAreaID, int StatusCode, string StatusMessage) Edit(CollectionAttribute collectionAttribute);
        void Delete(int collectionAttributeID);
    }
    public class CollectionAttributeProcessor(IUnitOfWork unitOfWork,
        DeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore) : IProcessCollectionAttribute
    {
        public (int CollectionAreaID, int StatusCode, string StatusMessage) Create(CollectionAttribute collectionAttribute)
        {
            if (string.IsNullOrWhiteSpace(collectionAttribute.CollectionAttributeName))
            {
                return (collectionAttribute.CollectionAreaID, 400, "Error_CollectionAttribute_NameMisssing");
            }
            CollectionAttributeSearchParameterModel searchParameterModel = new() { CollectionAttributeName = [collectionAttribute.CollectionAttributeName] };
            List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(CollectionAttribute)],
                FieldName = [nameof(CollectionAttribute.CollectionAttributeName)],
                TranslatedText = [collectionAttribute.CollectionAttributeName]
            }).Select(et => et.EntityId).Distinct()];
            if (entityIds.Count != 0)
            {
                searchParameterModel.CollectionAttributeID = entityIds;
            }
            CollectionAttribute? existingAttribute = GetListWithPredicate(searchParameterModel).FirstOrDefault();
            if (existingAttribute != null)
            {
                return (collectionAttribute.CollectionAreaID, 409, "Error_CollectionAttribute_NameDouble");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionAttribute newAttribute = new()
                {
                    CollectionAttributeName = translationService.SetIntoFallbackLanguage(collectionAttribute.CollectionAttributeName),
                    CollectionAttributeTypeInt = collectionAttribute.CollectionAttributeTypeInt,
                    RequiredAttribute = collectionAttribute.RequiredAttribute,
                    CollectionAreaID = collectionAttribute.CollectionAreaID
                };
                _ = unitOfWork.CollectionAttributeRepository.Insert(newAttribute);
                unitOfWork.Save();

                processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionAttribute),
                        EntityId = newAttribute.CollectionAttributeID,
                        FieldName = nameof(newAttribute.CollectionAttributeName),
                        TranslatedText = collectionAttribute.CollectionAttributeName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionAttribute.CollectionAttributeName);

                transactionScope.Complete();
                return (newAttribute.CollectionAttributeID, 201, "Success_CollectionAttribute_Created");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (collectionAttribute.CollectionAreaID, 500, "Error_Error_Ocurred");
            }
        }

        public void Delete(int collectionAttributeID)
        {
            throw new NotImplementedException();
        }

        public (int CollectionAreaID, int StatusCode, string StatusMessage) Edit(CollectionAttribute collectionAttribute)
        {
            if (collectionAttribute.CollectionAttributeID <= 0 ||
                string.IsNullOrWhiteSpace(collectionAttribute.CollectionAttributeName))
            {
                return (collectionAttribute.CollectionAreaID, 400, "Error_CollectionAttribute_NameMisssing");
            }

            CollectionAttributeSearchParameterModel searchParameter = new()
            {
                CollectionAttributeID = [collectionAttribute.CollectionAttributeID]
            };
            CollectionAttribute? existingAttribute = GetListWithPredicate(
                new CollectionAttributeSearchParameterModel{ CollectionAttributeID = [collectionAttribute.CollectionAttributeID]})
                .FirstOrDefault();
            if (existingAttribute == null)
            {
                return (collectionAttribute.CollectionAreaID, 501, "Error_CollectionAttribute_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                existingAttribute.CollectionAttributeName = translationService.SetIntoFallbackLanguage(collectionAttribute.CollectionAttributeName);
                existingAttribute.CollectionAttributeTypeInt = collectionAttribute.CollectionAttributeTypeInt;
                existingAttribute.RequiredAttribute = collectionAttribute.RequiredAttribute;
                unitOfWork.Save();

                processTranslations.Edit(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionAttribute),
                        EntityId = existingAttribute.CollectionAttributeID,
                        FieldName = nameof(existingAttribute.CollectionAttributeName),
                        TranslatedText = collectionAttribute.CollectionAttributeName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionAttribute.CollectionAttributeName);

                transactionScope.Complete();
                return (existingAttribute.CollectionAttributeID, 200, "Success_CollectionAttribute_Updated");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (collectionAttribute.CollectionAreaID, 500, "Error_Error_Ocurred");
            }
        }

        public List<CollectionAttribute> GetListWithPredicate(CollectionAttributeSearchParameterModel searchParameterModel)
        {
            IEnumerable<CollectionAttribute> query = unitOfWork.CollectionAttributeRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionAttribute>(searchParameterModel));

            List<CollectionAttribute> collectionAttributeList = [.. query];
            foreach (CollectionAttribute attribute in collectionAttributeList)
            {
                attribute.CollectionAttributeName = translationStore.GetTranslation(
                    nameof(CollectionAttribute),
                    attribute.CollectionAttributeID,
                    nameof(attribute.CollectionAttributeName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)) ?? attribute.CollectionAttributeName;
            }

            return [.. collectionAttributeList.OrderBy(x => x.CollectionAttributeName)];
        }
    }
}
