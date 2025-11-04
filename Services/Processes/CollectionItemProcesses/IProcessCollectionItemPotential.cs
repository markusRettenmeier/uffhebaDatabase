using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;

namespace Sammlerplattform.Services.Processes.CollectionItemProcesses
{
    public interface IProcessCollectionItemPotential
    {
        (int CollectionItemPotentialID, string StatusMessage) Create();
        //(int CollectionItemPotentialID, string StatusMessage) Update(CollectionItemPotential collectionItemPotential);
        (int CollectionItemPotentialID, string StatusMessage) Delete(CollectionItemPotential collectionItemPotential);
        List<CollectionItemPotential> GetWithPredicates(CollectionItemSearchParameterModel collectionItemSearchParameterModel);
    }

    public class CollectionItemPotentialProcessor(IUnitOfWork unitOfWork) : IProcessCollectionItemPotential
    {
        public (int CollectionItemPotentialID, string StatusMessage) Create()
        {
            CollectionItemPotential newcollectionItemPotential = new();
            newcollectionItemPotential = unitOfWork.CollectionItemPotentialRepository.Insert(newcollectionItemPotential);
            unitOfWork.Save();

            return (newcollectionItemPotential.CollectionItemPotentialID, "Collection item potential created successfully.");
        }

        //public (int CollectionItemPotentialID, string StatusMessage) Update(CollectionItemPotential collectionItemPotential)
        //{    

        //    return (collectionItemPotential.CollectionItemPotentialID, "Collection item potential updated successfully.");
        //}

        public (int CollectionItemPotentialID, string StatusMessage) Delete(CollectionItemPotential collectionItemPotential)
        {

            unitOfWork.CollectionItemPotentialRepository.Delete(collectionItemPotential);
            unitOfWork.Save();

            return (collectionItemPotential.CollectionItemPotentialID, "Collection item potential deleted successfully.");
        }

        public List<CollectionItemPotential> GetWithPredicates(CollectionItemSearchParameterModel collectionItemSearchParameterModel)
        {
            IEnumerable<CollectionItemPotential> query = unitOfWork.CollectionItemPotentialRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemPotential>(collectionItemSearchParameterModel),
                includeProperties: "CollectionItemEntityList.CollectionItemPictureList");

            return [.. query];
        }
    }
}
