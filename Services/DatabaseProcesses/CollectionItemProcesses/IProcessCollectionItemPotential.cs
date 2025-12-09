using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
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

            return (newcollectionItemPotential.CollectionItemPotentialID, "Success_CollectionItemPotential_Created");
        }

        public (int CollectionItemPotentialID, string StatusMessage) Delete(CollectionItemPotential collectionItemPotential)
        {

            unitOfWork.CollectionItemPotentialRepository.Delete(collectionItemPotential);
            unitOfWork.Save();

            return (collectionItemPotential.CollectionItemPotentialID, "Success_CollectionItemPotential_Deleted");
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
