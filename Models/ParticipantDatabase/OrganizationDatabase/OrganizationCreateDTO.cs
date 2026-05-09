using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase
{
    public class OrganizationCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PartyName_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<ConnectedPlaceDTO> ConnectedPlaceList { get; set; } = [];

        [Display(Name = "Industry", ResourceType = typeof(SharedResources))]
        public string? Industry { get; set; }
        public List<ConnectedEraDTO> ConnectedEraList { get; set; } = [];
    }
}
