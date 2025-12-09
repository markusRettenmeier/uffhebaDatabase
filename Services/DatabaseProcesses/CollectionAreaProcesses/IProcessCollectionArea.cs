using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses
{
    public interface IProcessCollectionArea
    {
        List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel);
        (int CollectionID, int StatusCode, string StatusMessage) Create(string collectionName);
        (int CollectionID, int StatusCode, string StatusMessage) Edit(CollectionArea collectionArea);
    }
    public class CollectionAreaProcessor(IUnitOfWork unitOfWork, 
        DeeplTranslationService translationService, 
        IProcessTranslations processTranslations, 
        ITranslationStore translationStore) : IProcessCollectionArea
    {
        public (int CollectionID, int StatusCode, string StatusMessage) Create(string collectionAreaName)
        {
            if (string.IsNullOrWhiteSpace(collectionAreaName))
            {
                return (0, 400, "Error_CollectionArea_NameEmpty");
            }

            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaName = [collectionAreaName] };
            List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(CollectionArea)],
                FieldName = [nameof(CollectionArea.CollectionAreaName)],
                TranslatedText = [collectionAreaName]
            }).Select(et => et.EntityId).Distinct()];
            if (entityIds.Count > 0)
            {
                collectionSearchParameterModel.CollectionAreaID = entityIds;
            }
            CollectionArea? existingCollection = GetListWithPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection != null)
            {
                return (0, 409, "Error_CollectionArea_Exists");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionArea newCollection = new() { CollectionAreaName = translationService.SetIntoFallbackLanguage(collectionAreaName) };
                newCollection = unitOfWork.CollectionAreaRepository.Insert(newCollection);
                unitOfWork.Save();

                Concept newConcept = new()
                {
                    ConceptName = translationService.SetIntoFallbackLanguage(newCollection.CollectionAreaName),
                    CollectionAreaID = newCollection.CollectionAreaID
                };
                newConcept = unitOfWork.ConceptRepository.Insert(newConcept);
                unitOfWork.Save();

                processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionArea),
                        EntityId = newCollection.CollectionAreaID,
                        FieldName = nameof(newCollection.CollectionAreaName),
                        TranslatedText = collectionAreaName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionAreaName);

                transactionScope.Complete();
                return (newCollection.CollectionAreaID, 201, "Success_CollectionArea_Created");
            }
            catch (Exception ex)
            {
                return (0, 500, "Error_Error_Ocurred");
            }
        }

        public (int CollectionID, int StatusCode, string StatusMessage) Edit(CollectionArea collectionArea)
        {
            if (collectionArea.CollectionAreaID <= 0)
            {
                return (0, 400, "Error_CollectionAreaID_Missing");
            }

            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaID = [collectionArea.CollectionAreaID] };
            CollectionArea? existingCollection = GetListWithPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection == null)
            {
                return (0, 404, "Error_CollectionArea_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                existingCollection.CollectionAreaName = translationService.SetIntoFallbackLanguage(collectionArea.CollectionAreaName);
                unitOfWork.Save();

                Concept? linkedConcept = unitOfWork.ConceptRepository.Get(
                    filter: c => c.CollectionAreaID == existingCollection.CollectionAreaID).FirstOrDefault();
                if (linkedConcept != null)
                {
                    linkedConcept.ConceptName = existingCollection.CollectionAreaName;
                    unitOfWork.Save();
                }

                processTranslations.Edit(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionArea),
                        EntityId = existingCollection.CollectionAreaID,
                        FieldName = nameof(existingCollection.CollectionAreaName),
                        TranslatedText = collectionArea.CollectionAreaName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionArea.CollectionAreaName);

                transactionScope.Complete();
                return (existingCollection.CollectionAreaID, 200, "Success_CollectionArea_Updated");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (0, 500, "Error_Error_Ocurred");
            }
        }

        public List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel)
        {
            IEnumerable<CollectionArea> query = unitOfWork.CollectionAreaRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionArea>(searchParameterModel),
                includeProperties: "CollectionAttributeList,ConceptList");

            List<CollectionArea> collectionAreaList = [.. query];
            foreach (CollectionArea collectionArea in collectionAreaList)
            {
                collectionArea.CollectionAreaName = translationStore.GetTranslation(
                        nameof(CollectionArea),
                        collectionArea.CollectionAreaID,
                        nameof(collectionArea.CollectionAreaName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? collectionArea.CollectionAreaName;
            }

            return [.. collectionAreaList.OrderBy(x => x.CollectionAreaName)];
        }
    }
}
