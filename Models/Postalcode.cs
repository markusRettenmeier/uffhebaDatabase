using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Postalcode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Postalcode_ID { get; set; }

        [StringLength(5)]
        [Required(ErrorMessage = "Bitte PLZ eingeben")]
        [Display(Name = "Postleitzahl")]
        public required string PostalcodeNumber { get; set; }
        public ICollection<City> CityICollection { get; set; } = [];
    }
}
