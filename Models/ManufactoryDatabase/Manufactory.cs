using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class Manufactory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Manufactory_ID { get; set; }

        [Required(ErrorMessage = "Bitte befüllen.")]
        [StringLength(100)]
        [Display(Name = "Manufakturname")]
        public required string ManufactoryName { get; set; }

        [Display(Name = "Standorte")]
        public ICollection<City> CityICollection { get; set; } = [];

        public int? ProductionFacility_ID { get; set; }

        [DisplayFormat(NullDisplayText = "Nicht angegeben")]
        public ProductionFacility? ProductionFacility { get; set; }
        public ICollection<BrickEntity> BrickEntityICollection { get; set; } = [];
    }
}