using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.PictureProcesses
{
    public interface IProcessCollectionItemPicture
    {
        (int PictureId, int Statuscode, string Statusmessage) Insert(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Update(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Delete(CollectionItemPicture collectionItemPicture);

    }

    public class CollectionItemPictureProcessor(IUnitOfWork unitOfWork) : IProcessCollectionItemPicture
    {
        public (int PictureId, int Statuscode, string Statusmessage) Insert(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity)
        {
            if (collectionItemPicture.IFormFile == null)
            {
                return (0, 302, "Error_File_Empty");
            }

            collectionItemPicture.CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID;
            CollectionItemPicture newCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.Insert(collectionItemPicture);
            unitOfWork.Save();

            return (newCollectionItemPicture.CollectionItemPictureID, 200, "Success_CollectionItemPicture_Created");
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity)
        {

            CollectionItemPicture? existingCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.GetByID(collectionItemPicture.CollectionItemPictureID);
            if (existingCollectionItemPicture == null)
            {
                return (302, "Error_CollectionItemPicture_NotFound");
            }

            if (existingCollectionItemPicture.PerspectiveInt != collectionItemPicture.PerspectiveInt)
            {
                existingCollectionItemPicture.PerspectiveInt = collectionItemPicture.PerspectiveInt;
                unitOfWork.Save();
            }

            return (200, "Success_CollectionItemPotential_Created");
        }
        public (int Statuscode, string Statusmessage) Delete(CollectionItemPicture collectionItemPicture)
        {
            CollectionItemPictureSearchParameterModel searchParameterModel = ParametersOperationToSearch(collectionItemPicture);
            CollectionItemPicture? existingCollectionItemPicture = GetWithPredicate(searchParameterModel);

            if (existingCollectionItemPicture == null)
            {
                return (404, "Error_CollectionItemPicture_NotFound");
            }

            unitOfWork.CollectionItemPictureRepository.Delete(collectionItemPicture);
            unitOfWork.Save();

            return (200, "Success_CollectionItemPicture_Deleted");
        }
        private CollectionItemPicture? GetWithPredicate(CollectionItemPictureSearchParameterModel searchParameterModel)
        {
            return unitOfWork.CollectionItemPictureRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemPicture>(searchParameterModel)).FirstOrDefault();
        }

        private static CollectionItemPictureSearchParameterModel ParametersOperationToSearch(CollectionItemPicture collectionItemPicture)
        {
            CollectionItemPictureSearchParameterModel searchParameterModel = new();
            searchParameterModel.CollectionItemPictureID.Add(collectionItemPicture.CollectionItemPictureID);
            searchParameterModel.CollectionItemEntityID.Add(collectionItemPicture.CollectionItemEntityID);
            return searchParameterModel;
        }
    }
}
