using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceNPlace
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PlaceNPlaceID { get; set; }
        public int PlaceID1 { get; set; }
        public Place Place1 { get; set; } = null!;
        public int PlaceID2 { get; set; }
        public Place Place2 { get; set; } = null!;
    }
}