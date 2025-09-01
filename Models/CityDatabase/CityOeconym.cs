using Sammlerplattform.Models.EraDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class CityOeconym
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CityOeconymID { get; set; }

        [Required]
        public int CityID { get; set; }
        public City City { get; set; } = null!;

        [Required]
        public int OeconymID { get; set; }
        public Oeconym Oeconym { get; set; } = null!;

        [Display(Name = "Aktueller Name")]
        public bool CurrentName { get; set; }

        public int? EraID { get; set; }
        public Era? Era { get; set; }
    }
}
