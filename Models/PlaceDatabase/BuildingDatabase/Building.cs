using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.BuildingDatabase
{
    public class Building
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BuildingID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
