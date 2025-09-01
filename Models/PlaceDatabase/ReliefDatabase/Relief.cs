using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.ReliefDatabase
{
    public class Relief
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ReliefID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
