using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ProductionFacility
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductionFacility_ID { get; set; }

        [Required(ErrorMessage = "Bitte befüllen.")]
        [StringLength(30)]
        [Display(Name = "Produktionsstätte")]
        [RegularExpression(@"^[a-zA-Z]{1,30}$", ErrorMessage = "Der Name darf nur Buchstaben und max. 30 Zeichen enthalten.")]
        public string ProductionFacilityName { get; set; } = string.Empty;

        public ICollection<Manufactory> ManufactoryICollection { get; set; } = [];
    }
}