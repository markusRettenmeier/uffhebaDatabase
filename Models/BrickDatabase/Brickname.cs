using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class Brickname
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BricknameID { get; set; }
        [StringLength(30)]
        [Display(Name = "Bezeichnung")]
        public required string Name { get; set; }
        public int? BrickPotentialID { get; set; }
        public BrickPotential? BrickPotential { get; set; }
        public string? Description { get; set; }
    }
}
