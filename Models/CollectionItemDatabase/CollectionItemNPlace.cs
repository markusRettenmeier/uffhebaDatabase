using Sammlerplattform.Models.PlaceDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNPlace
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemNPlaceID { get; set; }
        public int? CollectionItemEntityID { get; set; }
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        public int? CollectionItemPotentialID { get; set; }
        public CollectionItemPotential? CollectionItemPotential { get; set; }
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
        public string? Relationship { get; set; }
    }
}