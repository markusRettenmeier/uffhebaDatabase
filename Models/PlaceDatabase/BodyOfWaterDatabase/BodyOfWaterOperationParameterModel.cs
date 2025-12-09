using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase
{
    public class BodyOfWaterOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "BodyOfWater", ResourceType = typeof(SharedResources))]
        public BodyOfWater BodyOfWater { get; set; } = new();
    }
}
