using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase
{
    public class IndividualEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_ParticipantID_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_IndividualId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_ParticipantName_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "Pseudonym", ResourceType = typeof(SharedResources))]
        public string? Pseudonym { get; set; }

        [Display(Name = "Signature", ResourceType = typeof(SharedResources))]
        public string? Signature { get; set; }

        [Display(Name = "BirthYear", ResourceType = typeof(SharedResources))]
        public int? BirthYear { get; set; }

        [Display(Name = "DeathYear", ResourceType = typeof(SharedResources))]
        public int? DeathYear { get; set; }
        public List<ConnectedPlaceDTO> ConnectedPlaceList { get; set; } = [];
        public List<ConnectedEraDTO> ConnectedEraList { get; set; } = [];
    }
}