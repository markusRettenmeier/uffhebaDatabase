using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.OwnershipProofPictureDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.PictureProcesses
{
    public interface IProcessOwnershipProofPicture
    {
        (int Statuscode, string Statusmessage, int PictureId) Insert(OwnershipProofPicture ownershipProofPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Update(OwnershipProofPicture ownershipProofPicture, CollectionItemEntity collectionItemEntity);
        (int Statuscode, string Statusmessage) Delete(OwnershipProofPicture ownershipProofPicture);
    }

    public class OwnershipProofPictureProcessor(IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents) : IProcessOwnershipProofPicture
    {
        public (int Statuscode, string Statusmessage) Delete(OwnershipProofPicture ownershipProofPicture)
        {
            OwnershipProofPicture? existingOwnershipProofPicture = unitOfWork.OwnershipProofPictureRepository.GetByID(ownershipProofPicture.OwnershipProofPictureID);
            if (existingOwnershipProofPicture == null)
            {
                trackEvents.TrackError("OwnershipProofPictureProcessor.Delete: OwnershipProofPicture not found.", new Dictionary<string, object>
                {
                    { "OwnershipProofPicture", ownershipProofPicture }
                });
                return (302, "Error_OwnershipProofPicture_NotFound");
            }
            unitOfWork.OwnershipProofPictureRepository.Delete(existingOwnershipProofPicture);
            unitOfWork.Save();
            return (200, "Success_OwnershipProofPicture_Deleted");
        }

        public (int Statuscode, string Statusmessage, int PictureId) Insert(OwnershipProofPicture ownershipProofPicture, CollectionItemEntity collectionItemEntity)
        {
            if (ownershipProofPicture.IFormFile == null)
            {
                trackEvents.TrackError("OwnershipProofPictureProcessor.Insert: File is missing.", new Dictionary<string, object>
                {
                    { "OwnershipProofPicture", ownershipProofPicture },
                    { "CollectionItemEntity", collectionItemEntity }
                });
                return (302, "Error_File_Empty", 0);
            }
            ownershipProofPicture.CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID;
            OwnershipProofPicture newOwnershipProofPicture = unitOfWork.OwnershipProofPictureRepository.Insert(ownershipProofPicture);
            unitOfWork.Save();

            return (200, "Success_OwnershipProofPicture_Created", newOwnershipProofPicture.OwnershipProofPictureID);
        }

        public (int Statuscode, string Statusmessage) Update(OwnershipProofPicture ownershipProofPicture, CollectionItemEntity collectionItemEntity)
        {
            OwnershipProofPicture? existingOwnershipProofPicture = unitOfWork.OwnershipProofPictureRepository.GetByID(ownershipProofPicture.OwnershipProofPictureID);
            if (existingOwnershipProofPicture == null)
            {
                trackEvents.TrackError("OwnershipProofPictureProcessor.Update: OwnershipProofPicture not found.", new Dictionary<string, object>
                {
                    { "OwnershipProofPicture", ownershipProofPicture },
                    { "CollectionItemEntity", collectionItemEntity }
                });
                return (302, "Error_OwnershipProofPicture_NotFound");
            }

            bool isChanged = false;
            if (existingOwnershipProofPicture.OwnershipProofPictureTypeInt != ownershipProofPicture.OwnershipProofPictureTypeInt)
            {
                existingOwnershipProofPicture.OwnershipProofPictureTypeInt = ownershipProofPicture.OwnershipProofPictureTypeInt;
                isChanged = true;
            }
            if(existingOwnershipProofPicture.Signature != ownershipProofPicture.Signature)
            {
                existingOwnershipProofPicture.Signature = ownershipProofPicture.Signature;
                isChanged = true;
            }
            if (isChanged)
            {
                unitOfWork.Save();
            }

            return (200, "Success_OwnershipProofPicture_Updated");
        }
    }
}
