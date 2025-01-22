using Sammlerplattform.Models.ManufactoryDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.EraDatabase
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Era_ID { get; set; }

        [Display(Name = "Äralangbezeichnung")]
        [ConditionalRequired("IsEraLongRequired", true, ErrorMessage = "Bitte Bezeichnung angeben.")]
        [StringLength(50)]
        public string? EraLong { get; set; }

        [NotMapped]
        public bool IsEraLongRequired { get; set; } = true;

        [Display(Name = "Ärakurzbezeichnung")]
        [StringLength(10)]
        public string? EraShort { get; set; }

        //[Display(Name = "Startjahr")]
        //public int? StartYear { get; set; }

        //[Display(Name = "Endjahr")]
        //public int? EndYear { get; set; }
        //public ICollection<ManufacturingDate> ManufacturingDateICollection { get; set; } = [];
    }
}
