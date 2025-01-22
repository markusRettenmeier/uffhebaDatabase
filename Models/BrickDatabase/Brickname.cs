using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class Brickname
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Brickname_ID { get; set; }
        [StringLength(30)]
        public required string Name { get; set; }

        public int? BrickPotential_ID { get; set; }
        public BrickPotential? BrickPotential { get; set; }
    }
}
