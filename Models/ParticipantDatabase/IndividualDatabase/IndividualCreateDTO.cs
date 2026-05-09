using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase
{
    public class IndividualCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PartyName_Required", ErrorMessageResourceType = typeof(SharedResources))]
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
    public class ConnectedPlaceDTO
    {
        [Required(ErrorMessageResourceName = "Error_PlaceID_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        //[Required(ErrorMessageResourceName = "Error_Relationship_Required", ErrorMessageResourceType = typeof(SharedResources))]
        //public required string Relationship { get; set; }
    }
    public class ConnectedEraDTO
    {
        [Required(ErrorMessageResourceName = "Error_EraID_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }
    }
}
