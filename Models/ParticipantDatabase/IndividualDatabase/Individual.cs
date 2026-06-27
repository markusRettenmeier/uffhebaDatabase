using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase
{
    public class Individual
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IndividualID { get; set; }
        public string? Pseudonym { get; set; }
        public string? Signature { get; set; }
        public int ParticipantID { get; set; }
        public Participant Participant { get; set; } = null!;
    }
}
