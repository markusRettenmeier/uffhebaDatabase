using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceOperationParameterModel
    {
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = new();
        [Display(Name = "PlaceNToponymyList", ResourceType = typeof(SharedResources))]
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
        [Display(Name = "ChildPlaceList", ResourceType = typeof(SharedResources))]
        public List<Place> ChildPlaceList { get; set; } = [];
    }
}
