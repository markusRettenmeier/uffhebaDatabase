using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.CollectionAreaProcesses
{
    public interface IProcessCollectionAttribute
    {
        List<CollectionAttribute> GetListWithPredicate(CollectionAttributeSearchParameterModel searchParameterModel);
        (int CollectionAreaID, int StatusCode, string StatusMessage) Create(CollectionAttribute collectionAttribute);
        (int CollectionAreaID, int StatusCode, string StatusMessage) Edit(CollectionAttribute collectionAttribute);
        void Delete(int collectionAttributeID);
    }
    public class CollectionAttributeProcessor(IUnitOfWork unitOfWork) : IProcessCollectionAttribute
    {
        public (int CollectionAreaID, int StatusCode, string StatusMessage) Create(CollectionAttribute collectionAttribute)
        {
            if (string.IsNullOrWhiteSpace(collectionAttribute.CollectionAttributeName))
            {
                return (collectionAttribute.CollectionAreaID, 400, "Attributname darf nicht leer sein.");
            }

            CollectionAttributeSearchParameterModel searchParameter = new()
            {
                CollectionAttributeName = [collectionAttribute.CollectionAttributeName],
            };
            CollectionAttribute? existingAttribute = GetListWithPredicate(searchParameter).FirstOrDefault();
            if (existingAttribute != null)
            {
                return (collectionAttribute.CollectionAreaID, 409, "Attribut mit dem gleichen Namen existiert bereits.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionAttribute newAttribute = new()
                {
                    CollectionAttributeName = collectionAttribute.CollectionAttributeName,
                    CollectionAttributeTypeInt = collectionAttribute.CollectionAttributeTypeInt,
                    Required = collectionAttribute.Required,
                    CollectionAreaID = collectionAttribute.CollectionAreaID
                };
                _ = unitOfWork.CollectionAttributeRepository.Insert(newAttribute);
                unitOfWork.Save();

                transactionScope.Complete();
                return (newAttribute.CollectionAttributeID, 201, "Attribut erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (collectionAttribute.CollectionAreaID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
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
                return (collectionAttribute.CollectionAreaID, 400, "Ungültige Attributdaten.");
            }

            CollectionAttributeSearchParameterModel searchParameter = new()
            {
                CollectionAttributeID = [collectionAttribute.CollectionAttributeID]
            };
            CollectionAttribute? existingAttribute = GetListWithPredicate(searchParameter).FirstOrDefault();
            if (existingAttribute == null)
            {
                return (collectionAttribute.CollectionAreaID, 501, "Mermal existiert nicht.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                existingAttribute.CollectionAttributeName = collectionAttribute.CollectionAttributeName;
                existingAttribute.CollectionAttributeTypeInt = collectionAttribute.CollectionAttributeTypeInt;
                existingAttribute.Required = collectionAttribute.Required;
                unitOfWork.Save();

                transactionScope.Complete();
                return (existingAttribute.CollectionAttributeID, 200, "Attribut erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (collectionAttribute.CollectionAreaID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public List<CollectionAttribute> GetListWithPredicate(CollectionAttributeSearchParameterModel searchParameterModel)
        {
            IEnumerable<CollectionAttribute> query = unitOfWork.CollectionAttributeRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionAttribute>(searchParameterModel));

            return [.. query];
        }
    }
}
