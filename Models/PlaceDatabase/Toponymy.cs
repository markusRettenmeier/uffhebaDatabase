using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class Toponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ToponymyID { get; set; }

        [Required(ErrorMessage = "Bitte Toponymie-Name eingeben")]
        [StringLength(80)]
        [Display(Name = "Toponymie-Name (auch ehemalige)")]
        public required string ToponymyName { get; set; }

        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
    }
}
