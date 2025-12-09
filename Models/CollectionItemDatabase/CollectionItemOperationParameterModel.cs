using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemOperationParameterModel
    {
        public CollectionItemOperationParameterModel()
        {
            CollectionItemPictureList = [];
            CollectionAttributeValueList = [];
            CollectionItemEntity = new CollectionItemEntity() { UsingIdentityUsersID = string.Empty };
        }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        private bool _isPartOfASeries;
        private CollectionItemPotential _collectionItemPotential = new();

        [Display(Name = "IsPartOfASeries", ResourceType = typeof(SharedResources))]
        public bool IsPartOfASeries
        {
            get => _isPartOfASeries || IsCollectionItemPotentialFilled();
            set => _isPartOfASeries = value;
        }
        [Display(Name = "CollectionItemPotential", ResourceType = typeof(SharedResources))]
        public CollectionItemPotential CollectionItemPotential
        {
            get => _collectionItemPotential;
            set
            {
                _collectionItemPotential = value;
                // Optional: PropertyChanged event auslösen falls benötigt
            }
        }
        private bool IsCollectionItemPotentialFilled()
        {
            return CollectionItemPotential != null &&
                   CollectionItemPotential.CollectionItemPotentialID > 0;
        }

        [Display(Name = "CollectionItemPictureList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];
        [Display(Name = "CollectionItemNColorList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNColor> CollectionItemNColorList { get; set; } = [];
        [Display(Name = "CollectionItemNMaterialList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNMaterial> CollectionItemNMaterialList { get; set; } = [];
        [Display(Name = "StateList", ResourceType = typeof(SharedResources))]
        public List<State> StateList { get; set; } = [];
        [Display(Name = "ColorList", ResourceType = typeof(SharedResources))]
        public List<Color> ColorList { get; set; } = [];
        [Display(Name = "MaterialList", ResourceType = typeof(SharedResources))]
        public List<Material> MaterialList { get; set; } = [];
        [Display(Name = "CollectionItemNPartyList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];
        [Display(Name = "CollectionItemNPlaceList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        [Display(Name = "CollectionAttributeValueList", ResourceType = typeof(SharedResources))]
        public List<CollectionAttributeValue> CollectionAttributeValueList { get; set; } = [];
        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era Era { get; set; } = new() { EraName = string.Empty };
        [Display(Name = "Concept", ResourceType = typeof(SharedResources))]
        public Concept Concept { get; set; } = new() { ConceptName = string.Empty };
        [Display(Name = "CollectionAttributeList", ResourceType = typeof(SharedResources))]
        public List<CollectionAttribute> CollectionAttributeList { get; set; } = [];
    }
}
