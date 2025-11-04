using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;

namespace Sammlerplattform.Services.Processes.CollectionItemProcesses
{
    public interface IProcessCollectionItemValue
    {
        (int Statuscode, string Statusmessage) Insert(CollectionItemValue collectionItemValue, int collectionItemID);
        (int Statuscode, string Statusmessage) Update(CollectionItemValue collectionItemValue);
        (int Statuscode, string Statusmessage) Delete(int CollectionItemValueID);
    }

    public class CollectionItemValueProcessor(IUnitOfWork unitOfWork) : IProcessCollectionItemValue
    {
        public (int Statuscode, string Statusmessage) Insert(CollectionItemValue collectionItemValue, int collectionItemID)
        {
            if (collectionItemID <= 0)
            {
                return (400, "Invalid input parameters.");
            }

            collectionItemValue.CollectionItemEntityID = collectionItemID;
            _ = unitOfWork.CollectionItemValueRepository.Insert(collectionItemValue);
            unitOfWork.Save();

            return (200, "CollectionItemValue created successfully.");
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemValue collectionItemValue)
        {
            CollectionItemValue? existingCollectionItemValue = unitOfWork.CollectionItemValueRepository.Get(ci =>
                ci.CollectionItemValueID == collectionItemValue.CollectionItemValueID).FirstOrDefault();
            if (existingCollectionItemValue == null)
            {
                return (404, "CollectionItemValue not found.");
            }

            bool hasChanges = false;
            if (existingCollectionItemValue.ValueString != collectionItemValue.ValueString)
            {
                existingCollectionItemValue.ValueString = collectionItemValue.ValueString;
                hasChanges = true;
            }
            if (existingCollectionItemValue.ValueInt != collectionItemValue.ValueInt)
            {
                existingCollectionItemValue.ValueInt = collectionItemValue.ValueInt;
                hasChanges = true;
            }
            if (existingCollectionItemValue.ValueDate != collectionItemValue.ValueDate)
            {
                existingCollectionItemValue.ValueDate = collectionItemValue.ValueDate;
                hasChanges = true;
            }
            if (existingCollectionItemValue.ValueDecimal != collectionItemValue.ValueDecimal)
            {
                existingCollectionItemValue.ValueDecimal = collectionItemValue.ValueDecimal;
                hasChanges = true;
            }
            if (existingCollectionItemValue.ValueBool != collectionItemValue.ValueBool)
            {
                existingCollectionItemValue.ValueBool = collectionItemValue.ValueBool;
                hasChanges = true;
            }
            //if (existingCollectionItemValue.CollectionItemEntityID != collectionItemValue.CollectionItemEntityID)
            //{
            //    existingCollectionItemValue.CollectionItemEntityID = collectionItemValue.CollectionItemEntityID;
            //    hasChanges = true;
            //}
            //if (existingCollectionItemValue.CollectionItemPotentialID != collectionItemValue.CollectionItemPotentialID)
            //{
            //    existingCollectionItemValue.CollectionItemPotentialID = collectionItemValue.CollectionItemPotentialID;
            //    hasChanges = true;
            //}
            if (hasChanges)
                unitOfWork.Save();

            return (200, "CollectionItemValue updated successfully.");
        }

        public (int Statuscode, string Statusmessage) Delete(int CollectionItemValueID)
        {
            if (CollectionItemValueID <= 0)
            {
                return (400, "Invalid input parameters.");
            }

            CollectionItemValue? existingCollectionItemValue = unitOfWork.CollectionItemValueRepository.Get(ci =>
                ci.CollectionItemValueID == CollectionItemValueID).FirstOrDefault();
            if (existingCollectionItemValue == null)
            {
                return (404, "CollectionItemValue not found.");
            }
            unitOfWork.CollectionItemValueRepository.Delete(existingCollectionItemValue);
            unitOfWork.Save();

            return (200, "CollectionItemValue deleted successfully.");
        }
    }
}
