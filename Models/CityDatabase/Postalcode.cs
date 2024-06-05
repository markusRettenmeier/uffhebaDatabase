using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class Postalcode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Postalcode_ID { get; set; }

        [Required(ErrorMessage = "Bitte PLZ eingeben")]
        [StringLength(5)]
        [Display(Name = "Postleitzahl")]
        [RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Die PLZ darf nur Zahlen und max. 5 Zeichen enthalten.")]
        public required string PostalcodeNumber { get; set; }
        public ICollection<City> CityICollection { get; set; } = [];
    }
}
