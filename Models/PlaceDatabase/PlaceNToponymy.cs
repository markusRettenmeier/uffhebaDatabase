using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceNToponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PlaceNToponymyID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;

        [Required]
        public int ToponymyID { get; set; }
        public Toponymy Toponymy { get; set; } = null!;

        public bool IsCurrentName { get; set; }
    }
}
