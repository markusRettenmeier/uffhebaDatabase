using Sammlerplattform.Models.CityDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PersonDatabase
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

        [StringLength(50)]
        [Display(Name = "Künstlername")]
        public string? Pseudonym { get; set; }

        [StringLength(200)]
        [Display(Name = "Beschreibung")]
        public string? PersonDescription { get; set; }

        [Display(Name = "Preise")]
        public ICollection<Prize> PrizeICollection { get; set; } = [];

        [StringLength(30)]
        [Display(Name = "Signatur/Zeichen")]
        public string? PersonSignature { get; set; }
        //public ICollection<Image> ImageICollection { get; set; } = [];
        //public ICollection<Manufactory> ManufactoryICollection { get; set; } = [];
        public ICollection<Profession> ProfessionICollection { get; set; } = [];
    }
}

