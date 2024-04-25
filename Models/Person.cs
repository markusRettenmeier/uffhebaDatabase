using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Person_ID { get; set; }

        public string? Name { get; set; }

        [Display(Name = "Straße")]
        public string? Street { get; set; }

        [Display(Name = "Hausnummer")]
        public int? HouseNumber { get; set; }

        [Display(Name = "Ort")]
        public int? City_ID { get; set; }
        public City? City { get; set; }
    }
}
