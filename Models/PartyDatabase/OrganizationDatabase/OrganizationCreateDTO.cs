using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class OrganizationCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PartyTypeInt_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0, 1)]
        public int PartyTypeInt { get; set; }

        [Required(ErrorMessageResourceName = "Error_Party_NameMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<ConnectedPlaceCreateDTO> ConnectedPlaceList { get; set; } = [];

        [Display(Name = "Industry", ResourceType = typeof(SharedResources))]
        public string? Industry { get; set; }
        public int? IndustryId { get; set; }
    }
    public class ConnectedPlaceCreateDTO
    {
        public int PlaceID { get; set; }
    }
}
