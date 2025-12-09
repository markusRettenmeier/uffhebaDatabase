using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartyOperationParameterModel
    {

        [Display(Name = "Party", ResourceType = typeof(SharedResources))]
        public Party Party { get; set; } = new() { PartyName = string.Empty };
        [Display(Name = "PlaceList", ResourceType = typeof(SharedResources))]
        public List<Place> PlaceList { get; set; } = [];
    }
}
