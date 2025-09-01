using Sammlerplattform.Models.EraDatabase;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CityDatabase
{
    public class CityPostalcode
    {
        [Key]
        public int CityPostalcodeID { get; set; }

        [Required]
        public int CityID { get; set; }
        public City City { get; set; } = null!;

        [Required]
        public int PostalcodeID { get; set; }
        public Postalcode Postalcode { get; set; } = null!;

        public int? EraID { get; set; }
        public Era? Era { get; set; }
    }
}
