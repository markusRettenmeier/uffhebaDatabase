using Sammlerplattform.Models.BrickDatabase;
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

        [StringLength(50)]
        [Display(Name = "Künstlername")]
        public string? Pseudonym { get; set; }

        [StringLength(200)]
        [Display(Name = "Beschreibung")]
        public string? PersonDescription { get; set; }

        [Display(Name = "Preise")]
        public List<Prize> PrizeList { get; set; } = [];

        [StringLength(30)]
        [Display(Name = "Signatur/Zeichen")]
        public string? Signature { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public List<BrickEntityNPerson> BrickEntityNPersonList { get; set; } = [];
    }
}

