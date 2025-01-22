using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PersonDatabase
{
    public class Profession
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Profession_ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Berufsbezeichnung")]
        public required string Name { get; set; }
        public ICollection<Person> PersonICollection { get; set; } = [];
    }
}
