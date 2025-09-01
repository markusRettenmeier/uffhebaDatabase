using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class Oeconym
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int OeconymID { get; set; }

        [Required(ErrorMessage = "Bitte Ortsname eingeben")]
        [StringLength(80)]
        [Display(Name = "Ortsname (auch alt)")]
        [RegularExpression(@"^[a-zA-Z]{1,50}$", ErrorMessage = "Der Ortsname darf nur Buchstaben und max. 50 Zeichen enthalten.")]
        public required string OeconymName { get; set; }
        public List<CityOeconym> CityOeconymList { get; set; } = [];
    }
}