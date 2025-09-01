using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickEntityNManufactoryNCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BrickEntityNManufactoryNCityID { get; set; }

        public int BrickEntityID { get; set; }
        public BrickEntity BrickEntity { get; set; } = null!;
        public int ManufactoryID { get; set; }
        public Manufactory Manufactory { get; set; } = null!;
        public int? CityID { get; set; }
        public City? City { get; set; }
    }
}
