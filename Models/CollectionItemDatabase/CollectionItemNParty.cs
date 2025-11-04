using Sammlerplattform.Models.PartyDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNParty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemNPartyID { get; set; }
        public int? CollectionItemEntityID { get; set; }
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        public int? CollectionItemPotentialID { get; set; }
        public CollectionItemPotential? CollectionItemPotential { get; set; }
        public int PartyID { get; set; }
        public Party Party { get; set; } = null!;
        public string? Relationship { get; set; }
    }
}