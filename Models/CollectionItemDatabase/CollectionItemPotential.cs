using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemPotential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemPotentialID { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        public List<CollectionItemValue> CollectionItemValueList { get; set; } = [];
    }
}