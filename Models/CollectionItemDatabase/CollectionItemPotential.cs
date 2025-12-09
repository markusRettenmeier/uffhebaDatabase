using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemPotential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemPotentialID", ResourceType = typeof(SharedResources))]
        public int CollectionItemPotentialID { get; set; }

        [Display(Name = "ProductionSize", ResourceType = typeof(SharedResources))]
        public int? ProductionSize { get; set; }
        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        [Display(Name = "CollectionItemNPartyList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];
        [Display(Name = "CollectionItemNPlaceList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        [Display(Name = "CollectionAttributeValueList", ResourceType = typeof(SharedResources))]
        public List<CollectionAttributeValue> CollectionAttributeValueList { get; set; } = [];
    }
}