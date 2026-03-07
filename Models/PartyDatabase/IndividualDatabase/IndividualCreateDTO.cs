using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PartyDatabase.IndividualDatabase
{
    public class IndividualCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PartyTypeInt_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0, 1)]
        public int PartyTypeInt { get; set; }

        [Required(ErrorMessageResourceName = "Error_Party_NameMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "Pseudonym", ResourceType = typeof(SharedResources))]
        public string? Pseudonym { get; set; }

        [Display(Name = "Signature", ResourceType = typeof(SharedResources))]
        public string? Signature { get; set; }
        public List<ConnectedPlaceCreateDTO> ConnectedPlaceList { get; set; } = [];
    }
    public class ConnectedPlaceCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PlaceID_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
    }
}
