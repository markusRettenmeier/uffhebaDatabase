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
        [Display(Name = "ParticipantID", ResourceType = typeof(SharedResources))]
        public int ParticipantID { get; set; }

        [Display(Name = "ParticipantName", ResourceType = typeof(SharedResources))]
        public required string ParticipantName { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }

        [Display(Name = "ParticipantType", ResourceType = typeof(SharedResources))]
        public int ParticipantTypeInt { get; set; }

        [NotMapped]
        public ParticipantType ParticipantTypeEnum
        {
            get => (ParticipantType)ParticipantTypeInt;
            set => ParticipantTypeInt = (int)value;
        }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "Individual", ResourceType = typeof(SharedResources))]
        public Individual? Individual { get; set; }

        [Display(Name = "Organization", ResourceType = typeof(SharedResources))]
        public Organization? Organization { get; set; }
        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];

        [Display(Name = "ConnectedPlaces", ResourceType = typeof(SharedResources))]
        public List<ParticipantNPlace> ParticipantNPlaceList { get; set; } = [];

        [Display(Name = "ConnectedEras", ResourceType = typeof(SharedResources))]
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
