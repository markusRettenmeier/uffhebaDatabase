using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PersonDatabase
{
    public class Prize
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Prize_ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Preis")]
        public required string Name { get; set; }

        [StringLength(50)]
        [Display(Name = "Kategorie")]
        public string? Category { get; set; }
        public List<Person> PersonList { get; set; } = [];
    }
}
