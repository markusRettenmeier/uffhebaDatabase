using Sammlerplattform.Models.EraDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantNEra
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ParticipantNEraId { get; set; }
        public int ParticipantId { get; set; }
        public Participant Participant { get; set; } = null!;
        public int EraId { get; set; }
        public Era Era { get; set; } = null!;
    }
}