using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.PictureProcesses
{
    public interface IProcessCollectionItemPicture
    {
        (int Statuscode, string Statusmessage, int PictureId) Insert(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Update(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Delete(CollectionItemPicture collectionItemPicture);

    }

    public class CollectionItemPictureProcessor(IUnitOfWork unitOfWork
        , ITrackEvents trackEvents) : IProcessCollectionItemPicture
    {
        public (int Statuscode, string Statusmessage, int PictureId) Insert(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity)
        {
            if (collectionItemPicture.IFormFile == null)
            {
                trackEvents.TrackWarning("CollectionItemPictureProcessor.Insert: File is missing.", new Dictionary<string, object>
                {
                    { "CollectionItemPicture", collectionItemPicture },
                    { "CollectionItemEntity", collectionItemEntity }
                });
                return (302, "Error_File_Empty", 0);
            }

            collectionItemPicture.CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID;
            CollectionItemPicture newCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.Insert(collectionItemPicture);
            unitOfWork.Save();

            return (200, "Success_CollectionItemPicture_Created", newCollectionItemPicture.CollectionItemPictureID);
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity)
        {

            CollectionItemPicture? existingCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.GetByID(collectionItemPicture.CollectionItemPictureID);
            if (existingCollectionItemPicture == null)
            {
                trackEvents.TrackWarning("CollectionItemPictureProcessor.Update: CollectionItemPicture not found.", new Dictionary<string, object>
                {
                    { "CollectionItemPicture", collectionItemPicture },
                    { "CollectionItemEntity", collectionItemEntity }
                });
                return (302, "Error_CollectionItemPicture_NotFound");
            }

            if (existingCollectionItemPicture.PerspectiveInt != collectionItemPicture.PerspectiveInt)
            {
                existingCollectionItemPicture.PerspectiveInt = collectionItemPicture.PerspectiveInt;
                unitOfWork.Save();
            }

            return (200, "Success_CollectionItemPicture_Created");
        }
        public (int Statuscode, string Statusmessage) Delete(CollectionItemPicture collectionItemPicture)
        {
            CollectionItemPictureSearchParameterModel searchParameterModel = ParametersOperationToSearch(collectionItemPicture);
            CollectionItemPicture? existingCollectionItemPicture = GetWithPredicate(searchParameterModel);

            if (existingCollectionItemPicture == null)
            {
                trackEvents.TrackWarning("CollectionItemPictureProcessor.Delete: CollectionItemPicture not found.", new Dictionary<string, object>
                {
                    { "CollectionItemPicture", collectionItemPicture }
                });
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
