using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase
{
    public class OrganizationEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_ParticipantID_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_OrganizationId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_ParticipantName_Required", ErrorMessageResourceType = typeof(SharedResources))]
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
