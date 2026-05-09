using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceCreateDTO
    {
        public List<ToponymyCreateDTO> ToponymyList { get; set; } = [];
        public List<ConnectedPlace> ConnectedPlaceList { get; set; } = [];
        public string? FurtherSpecs { get; set; }
        public string? WikipediaUrl { get; set; }
    }
    public class ToponymyCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PlaceName_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Name { get; set; }
        public bool IsCurrentName { get; set; }
    }
    public class ConnectedPlace
    {
        public int PlaceID { get; set; }
    }
}
