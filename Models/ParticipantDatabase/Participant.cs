using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class Participant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ParticipantID { get; set; }
        public required string ParticipantName { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int ParticipantTypeInt { get; set; }
        public string? WikipediaUrl { get; set; }
        public Individual? Individual { get; set; }
        public Organization? Organization { get; set; }
        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
        public List<ParticipantNPlace> ParticipantNPlaceList { get; set; } = [];
        public List<ParticipantNEra> ParticipantNEraList { get; set; } = [];
    }

    public enum ParticipantType
    {
        [Display(Name = "Individual", ResourceType = typeof(SharedResources))]
        Individual = 0,
        [Display(Name = "Organization", ResourceType = typeof(SharedResources))]
        Organization = 1
    }
}
