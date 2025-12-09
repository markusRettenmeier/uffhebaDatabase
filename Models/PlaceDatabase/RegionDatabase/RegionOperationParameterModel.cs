using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.RegionDatabase
{
    public class RegionOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "Region", ResourceType = typeof(SharedResources))]
        public Region Region { get; set; } = new();
    }
}
