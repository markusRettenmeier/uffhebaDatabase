using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantDisplayDTO
    {
        [Display(Name = "Pseudonym", ResourceType = typeof(SharedResources))]
        public string? Pseudonym { get; set; }
        [Display(Name = "Signature", ResourceType = typeof(SharedResources))]
        public string? Signature { get; set; }

        public int? IndustryId { get; set; }
        [Display(Name = "Industry", ResourceType = typeof(SharedResources))]
        public string? IndustryName { get; set; }

        public int ParticipantID { get; set; }

        [Display(Name = "ParticipantName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "StartYear", ResourceType = typeof(SharedResources))]
        public int? StartYear { get; set; }

        [Display(Name = "EndYear", ResourceType = typeof(SharedResources))]
        public int? EndYear { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "ConnectedPlaces", ResourceType = typeof(SharedResources))]
        public IEnumerable<PlaceDisplayDTO> ConnectedPlaceList { get; set; } = []; 
        [Display(Name = "ParticipantType", ResourceType = typeof(SharedResources))]
        public int ParticipantTypeInt { get; set; }
        public ParticipantType ParticipantTypeEnum
        {
            get => (ParticipantType)ParticipantTypeInt;
            set => ParticipantTypeInt = (int)value;
        }

        [Display(Name = "ConnectedEras", ResourceType = typeof(SharedResources))]
        public IEnumerable<EraDisplayDTO> ConnectedEraList { get; set; } = [];
        public IEnumerable<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
    }
}
