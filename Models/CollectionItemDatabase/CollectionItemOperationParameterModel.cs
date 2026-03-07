using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.OwnershipProofPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemOperationParameterModel
    {
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        public CollectionSet CollectionSet { get; set; } = new() { CollectionSetName = string.Empty };

        private bool _isPartOfASet;
        [Display(Name = "IsPartOfASet", ResourceType = typeof(SharedResources))]
        public bool IsPartOfASet { get => _isPartOfASet || IsSetFilled(); set => _isPartOfASet = value; }
        private bool IsSetFilled()
        {
            return CollectionSet != null &&
                   CollectionSet.CollectionSetId > 0;
        }

        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];
        public List<OwnershipProofPicture> OwnershipProofPictureList { get; set; } = [];
        public List<StatePreservation> StatePreservationList { get; set; } = [];
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        public List<ConceptValue> ConceptValueList { get; set; } = [];

        public List<ConceptViewModel> CvmList { get; set; } = [];

        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era Era { get; set; } = new() { EraName = string.Empty };
    }
}
