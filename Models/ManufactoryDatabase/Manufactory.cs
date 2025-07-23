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
        public int ManufactoryID { get; set; }

        [Required(ErrorMessage = "Bitte befüllen.")]
        [StringLength(100)]
        [Display(Name = "Manufakturname")]
        public required string ManufactoryName { get; set; }

        [Display(Name = "Standorte")]
        public List<City> CityList { get; set; } = [];

        public int? ProductionFacility_ID { get; set; }

        [DisplayFormat(NullDisplayText = "Nicht angegeben")]
        public ProductionFacility? ProductionFacility { get; set; }
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
    }
}