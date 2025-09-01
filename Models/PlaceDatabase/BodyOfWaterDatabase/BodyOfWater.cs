using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase
{
    public class BodyOfWater
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BodyOfWaterID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
