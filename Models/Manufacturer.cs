using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Manufacturer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Manufacturer_ID { get; set; }

        [Required(ErrorMessage = "Bitte befüllen.")]
        [StringLength(100)]
        [Display(Name = "Verlag")]
        public string ManufacturerName { get; set; } = string.Empty;

        [Display(Name = "Ort")]
        public List<City>? CityList { get; set; } = [];
    }
}
