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

        [Display(Name = "Aktueller Name")]
        public bool IsCurrentName { get; set; }

        //public int? EraID { get; set; }
        //public Era? Era { get; set; }
    }
}
