using Sammlerplattform.Resources;
using Sammlerplattform.Models.PlaceDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNPlace
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemNPlaceID", ResourceType = typeof(SharedResources))]
        public int CollectionItemNPlaceID { get; set; }
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemEntityID { get; set; }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        [Display(Name = "CollectionItemPotentialID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemPotentialID { get; set; }
        [Display(Name = "CollectionItemPotential", ResourceType = typeof(SharedResources))]
        public CollectionItemPotential? CollectionItemPotential { get; set; }
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;
        [Display(Name = "Relationship", ResourceType = typeof(SharedResources))]
        public string? Relationship { get; set; }
    }
}