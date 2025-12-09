using Sammlerplattform.Resources;
using Sammlerplattform.Models.PartyDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNParty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemNPartyID", ResourceType = typeof(SharedResources))]
        public int CollectionItemNPartyID { get; set; }
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemEntityID { get; set; }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        [Display(Name = "CollectionItemPotentialID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemPotentialID { get; set; }
        [Display(Name = "CollectionItemPotential", ResourceType = typeof(SharedResources))]
        public CollectionItemPotential? CollectionItemPotential { get; set; }
        [Display(Name = "PartyID", ResourceType = typeof(SharedResources))]
        public int PartyID { get; set; }
        [Display(Name = "Party", ResourceType = typeof(SharedResources))]
        public Party Party { get; set; } = null!;
        [Display(Name = "Relationship", ResourceType = typeof(SharedResources))]
        public required string Relationship { get; set; }
    }
}