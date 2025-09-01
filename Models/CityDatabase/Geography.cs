using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class Geography
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Geography_ID { get; set; }

        [StringLength(20)]
        [Display(Name = "Geografischer Name")]
        public required string GeographyName { get; set; }
        public List<City> CityList { get; set; } = [];
    }
}