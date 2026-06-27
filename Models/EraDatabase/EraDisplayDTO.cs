using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.EraDatabase
{
    public class EraDisplayDTO
    {
        public int EraID { get; set; }
        public string EraName { get; set; } = string.Empty;

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public List<ParticipantNEra> ParticipantNEraList { get; set; } = [];
    }
}
