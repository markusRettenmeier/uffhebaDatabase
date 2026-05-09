using Sammlerplattform.Models.ParticipantDatabase;
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
        [Display(Name = "Pseudonym", ResourceType = typeof(SharedResources))]
        public string? Pseudonym { get; set; }
        [Display(Name = "Signature", ResourceType = typeof(SharedResources))]
        public string? Signature { get; set; }
        public int ParticipantID { get; set; }
        public Participant Participant { get; set; } = null!;
    }
}
