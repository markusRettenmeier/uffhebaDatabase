using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class Geography
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Geography_ID { get; set; }

        //[ConditionalRequired("IsGeographyNameRequired", true, ErrorMessage = "Bitte geographischen Namen eingeben")]
        [StringLength(20)]
        [Display(Name = "Geografischer Name")]
        public string? GeographyName { get; set; }

        [NotMapped]
        public bool IsGeographyNameRequired { get; set; } = true;
        public ICollection<City> CityICollection { get; set; } = [];
    }
}