using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Oeconym
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Oeconym_ID { get; set; }

        [Required(ErrorMessage = "Bitte Name eingeben")]
        [StringLength(50)]
        [Display(Name = "Ortsname (auch alt)")]
        public required string OeconymName { get; set; }
        public ICollection<CityNOeconym> CityNOeconyms { get; set; } = [];
    }
}
