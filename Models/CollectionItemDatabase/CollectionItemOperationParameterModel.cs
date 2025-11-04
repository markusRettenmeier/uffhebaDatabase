using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemOperationParameterModel
    {
        public CollectionItemOperationParameterModel()
        {
            CollectionItemPictureList = [];
            CollectionItemValueList = [];
            CollectionItemEntity = new CollectionItemEntity() { UsingIdentityUsersID = string.Empty };
        }
        public CollectionItemEntity CollectionItemEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        //public bool IsPartOfASeries { get; set; }
        //public CollectionItemPotential CollectionItemPotential { get; set; } = new();
        private bool _isPartOfASeries;
        private CollectionItemPotential _collectionItemPotential = new();
        public bool IsPartOfASeries
        {
            get => _isPartOfASeries || IsCollectionItemPotentialFilled();
            set => _isPartOfASeries = value;
        }
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

        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];
        public List<CollectionItemNColor> CollectionItemNColorList { get; set; } = [];
        public List<CollectionItemNMaterial> CollectionItemNMaterialList { get; set; } = [];
        public List<State> StateList { get; set; } = [];
        public List<Color> ColorList { get; set; } = [];
        public List<Material> MaterialList { get; set; } = [];
        public ProcessOfManufacture ProcessOfManufacture { get; set; } = new() { ProcessOfManufactureName = string.Empty, Mainprocess = string.Empty };
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        public List<CollectionItemValue> CollectionItemValueList { get; set; } = [];
        public Era Era { get; set; } = new() { EraName = string.Empty };
        public Concept Concept { get; set; } = new() { ConceptName = string.Empty };
        public List<CollectionAttribute> CollectionAttributeList { get; set; } = [];
    }
}
