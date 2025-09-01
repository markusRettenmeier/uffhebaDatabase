using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceOperationParameterModel
    {
        public Place Place { get; set; } = new();
        [Display(Name = "Ortsname")]
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
        public List<Place> ChildPlaceList { get; set; } = [];
    }
}
