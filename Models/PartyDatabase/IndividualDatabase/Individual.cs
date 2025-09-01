using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.IndividualDatabase
{
    public class Individual
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IndividualID { get; set; }
        public string? Pseudonym { get; set; }
        public string? Signature { get; set; }
        public int PartyID { get; set; }
        public Party Party { get; set; } = null!;
    }
}
