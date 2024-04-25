using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class CityNOeconym
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CityNOeconym_ID { get; set; }
        public int City_ID { get; set; }
        public City City { get; set; } = new();
        public int Oeconym_ID { get; set; }
        public Oeconym Oeconym { get; set; } = new() { OeconymName = "" };

        [Display(Name ="Aktueller Name")]
        public bool CurrentName { get; set; }
    }
}
