using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase
{
    public class TransportRouteOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "TransportRoute", ResourceType = typeof(SharedResources))]
        public TransportRoute TransportRoute { get; set; } = new();
    }
}
