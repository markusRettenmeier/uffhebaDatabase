using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Printing
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Printing_ID { get; set; }

        [Display(Name = "Druckverfahren")]
        public int? Technique { get; set; }

        [Display(Name = "Druckausführung")]
        public int? Style { get; set; }
    }
}
