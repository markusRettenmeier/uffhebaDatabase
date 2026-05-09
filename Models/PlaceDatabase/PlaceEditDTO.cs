using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_PlaceID_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_PlaceId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "FurtherSpecs", ResourceType = typeof(SharedResources))]
        public string? FurtherSpecs { get; set; }
        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<PlaceNToponymyEditDTO> ToponymyList { get; set; } = [];
        public List<ConnectedPlace> ConnectedPlaceList { get; set; } = [];
    }
    public class PlaceNToponymyEditDTO
    {
        public int? PlaceNToponymyID { get; set; }

        [Required(ErrorMessageResourceName = "Error_PlaceName_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Name { get; set; }
        [Display(Name = "IsCurrentName", ResourceType = typeof(SharedResources))]
        public bool IsCurrentName { get; set; }
    }
}
