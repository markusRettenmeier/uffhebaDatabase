using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemDisplayDTO
    {
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];
        //public List<OwnershipProofPicture> OwnershipProofPictureList { get; set; } = [];
        public List<StatePreservation> StatePreservationList { get; set; } = [];

        [Display(Name = "Participants", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];

        [Display(Name = "ConnectedPlaces", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        public List<ConceptValue> ConceptValueList { get; set; } = [];

        public List<ConceptViewModel> CvmList { get; set; } = [];

        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era Era { get; set; } = new() { EraName = string.Empty };

        [Display(Name = "DisplayName", ResourceType = typeof(SharedResources))]
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(CollectionItemEntity.UniqueName))
                    return CollectionItemEntity.UniqueName;
                else if (!string.IsNullOrEmpty(CollectionItemEntity.PersonalIdentificationNumber))
                    return CollectionItemEntity.PersonalIdentificationNumber;
                else
                    return CollectionItemEntity.CollectionItemEntityID.ToString();
            }
        }
    }
}
