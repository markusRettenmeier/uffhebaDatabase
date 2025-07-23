using Sammlerplattform.Models.BrickDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.EraDatabase
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ÄraId")]
        public int EraID { get; set; }

        [Display(Name = "Äralangbezeichnung")]
        //[ConditionalRequired("IsEraLongRequired", true, ErrorMessage = "Bitte Bezeichnung angeben.")]
        [StringLength(50)]
        public required string EraName { get; set; }

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

        public List<BrickEntity> BrickEntityList { get; set; } = [];
    }
}
