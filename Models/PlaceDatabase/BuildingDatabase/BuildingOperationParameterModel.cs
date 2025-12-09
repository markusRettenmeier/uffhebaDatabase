using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.BuildingDatabase
{
    public class BuildingOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "Building", ResourceType = typeof(SharedResources))]
        public Building Building { get; set; } = new();
    }
}
