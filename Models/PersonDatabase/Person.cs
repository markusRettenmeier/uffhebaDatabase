using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.PostcardDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PersonDatabase
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PersonID { get; set; }

        public required string Name { get; set; }

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
        public string? Signature { get; set; }
        //public ICollection<Image> ImageICollection { get; set; } = [];
        public ICollection<Profession> ProfessionICollection { get; set; } = [];
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public List<BrickEntityNPerson> BrickEntityNPersonList { get; set; } = [];
    }
}

