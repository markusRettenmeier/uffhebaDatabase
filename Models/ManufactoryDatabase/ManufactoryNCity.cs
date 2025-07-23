using Sammlerplattform.Models.CityDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ManufactoryNCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ManufactoryNCityID { get; set; }
        public int ManufactoryID { get; set; }
        public Manufactory Manufactory { get; set; } = null!;
        public int CityID { get; set; }
        public City City { get; set; } = null!;
    }
}
