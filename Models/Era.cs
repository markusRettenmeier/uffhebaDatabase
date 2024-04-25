using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Era_ID { get; set; }

        [Display(Name = "Äralangbezeichnung")]
        [ConditionalRequired("IsEraLongRequired", true, ErrorMessage ="Bitte Bezeichnung angeben.")]
        [StringLength(50)]
        public string? EraLong { get; set; }

        [NotMapped]
        public bool IsEraLongRequired { get; set; } = true;

        [Display(Name = "Ärakurzbezeichnung")]
        [StringLength(10)]
        public string? EraShort { get; set; }
    }
}
