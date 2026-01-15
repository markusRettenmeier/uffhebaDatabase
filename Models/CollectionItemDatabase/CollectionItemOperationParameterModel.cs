using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
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

        [Display(Name = "CollectionItemPictureList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];

        [Display(Name = "StatePreservationList", ResourceType = typeof(SharedResources))]
        public List<StatePreservation> StatePreservationList { get; set; } = [];

        [Display(Name = "CollectionItemNPartyList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];

        [Display(Name = "CollectionItemNPlaceList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];

        [Display(Name = "ConceptValueList", ResourceType = typeof(SharedResources))]
        public List<ConceptValue> ConceptValueList { get; set; } = [];

        [Display(Name = "ConceptList", ResourceType = typeof(SharedResources))]
        public List<Concept> ConceptList { get; set; } = [];

        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era Era { get; set; } = new() { EraName = string.Empty };

        [Display(Name = "Concept", ResourceType = typeof(SharedResources))]
        public Concept Concept { get; set; } = new() { Name = string.Empty };
    }
}
