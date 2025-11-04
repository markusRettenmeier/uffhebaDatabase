using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.CollectionAreaProcesses
{
    public interface IProcessCollectionArea
    {
        List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel);
        (int CollectionID, int StatusCode, string StatusMessage) Create(string collectionName);
        (int CollectionID, int StatusCode, string StatusMessage) Edit(CollectionArea collectionArea);
    }
    public class CollectionAreaProcessor(IUnitOfWork unitOfWork) : IProcessCollectionArea
    {
        public (int CollectionID, int StatusCode, string StatusMessage) Create(string collectionAreaName)
        {
            if (string.IsNullOrWhiteSpace(collectionAreaName))
            {
                return (0, 400, "Collection name cannot be empty.");
            }

            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaName = [collectionAreaName] };
            CollectionArea? existingCollection = GetListWithPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection != null)
            {
                return (0, 409, "Collection with the same name already exists.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionArea newCollection = new() { CollectionAreaName = collectionAreaName };
                newCollection = unitOfWork.CollectionAreaRepository.Insert(newCollection);
                unitOfWork.Save();

                Concept newConcept = new()
                {
                    ConceptName = newCollection.CollectionAreaName,
                    CollectionAreaID = newCollection.CollectionAreaID
                };
                newConcept = unitOfWork.ConceptRepository.Insert(newConcept);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newCollection.CollectionAreaID, 201, "Sammlung erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
        public (int CollectionID, int StatusCode, string StatusMessage) Edit(CollectionArea collectionArea)
        {
            if (collectionArea.CollectionAreaID <= 0)
            {
                return (0, 400, "SammlungsID kann nicht leer sein.");
            }

            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaID = [collectionArea.CollectionAreaID] };
            CollectionArea? existingCollection = GetListWithPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection == null)
            {
                return (0, 404, "Collection not found.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                existingCollection.CollectionAreaName = collectionArea.CollectionAreaName;
                unitOfWork.Save();

                transactionScope.Complete();
                return (existingCollection.CollectionAreaID, 200, "Sammlung erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel)
        {
            IEnumerable<CollectionArea> query = unitOfWork.CollectionAreaRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionArea>(searchParameterModel),
                includeProperties: "CollectionAttributeList,ConceptList");

            return [.. query.OrderBy(x => x.CollectionAreaName)];
        }
    }
}
