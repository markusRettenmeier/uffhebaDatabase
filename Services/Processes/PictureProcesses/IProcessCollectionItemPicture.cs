using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Services.Processes.PictureProcesses
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
            if (collectionItemPicture.Datei == null)
            {
                return (0, 302, "Bild leer.");
            }

            collectionItemPicture.CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID;
            CollectionItemPicture newCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.Insert(collectionItemPicture);
            unitOfWork.Save();

            return (newCollectionItemPicture.CollectionItemPictureID, 200, "Bild erstellt.");
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemPicture collectionItemPicture, CollectionItemEntity collectionItemEntity)
        {

            CollectionItemPicture? existingCollectionItemPicture = unitOfWork.CollectionItemPictureRepository.GetByID(collectionItemPicture.CollectionItemPictureID);
            if (existingCollectionItemPicture == null)
            {
                return (302, "Eintrag des Bildes in Datanbank nicht gefunden.");
            }

            if (existingCollectionItemPicture.PerspectiveInt != collectionItemPicture.PerspectiveInt)
            {
                existingCollectionItemPicture.PerspectiveInt = collectionItemPicture.PerspectiveInt;
                unitOfWork.Save();
            }

            return (200, "Änderung erfolgreich.");
        }
        public (int Statuscode, string Statusmessage) Delete(CollectionItemPicture collectionItemPicture)
        {
            CollectionItemPictureSearchParameterModel searchParameterModel = ParametersOperationToSearch(collectionItemPicture);
            CollectionItemPicture? existingCollectionItemPicture = GetWithPredicate(searchParameterModel);

            if (existingCollectionItemPicture == null)
            {
                return (404, "Bild nicht gefunden.");
            }

            unitOfWork.CollectionItemPictureRepository.Delete(collectionItemPicture);
            unitOfWork.Save();

            return (200, "Bild gelöscht.");
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
