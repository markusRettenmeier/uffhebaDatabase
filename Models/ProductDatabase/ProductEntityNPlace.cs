using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductEntityNPlace
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductEntityNPlaceID { get; set; }
        public int? ProductEntityID { get; set; }
        public BrickEntity? BrickEntity { get; set; }
        public int? ProductPotentialID { get; set; }
        public BrickPotential? BrickPotenital { get; set; }
        public int? PlaceID { get; set; }
        public Place Place { get; set; } = null!;
        public string? Relationship { get; set; }
    }
}