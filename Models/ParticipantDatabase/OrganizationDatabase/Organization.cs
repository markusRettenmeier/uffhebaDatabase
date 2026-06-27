using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase.IndustryDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int OrganizationID { get; set; }
        public int ParticipantID { get; set; }
        public Participant Participant { get; set; } = null!;
        public int? IndustryID { get; set; }
        public Industry? Industry { get; set; }
    }
}
