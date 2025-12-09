using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.EraDatabase
{
    public class EraOperationParameterModel
    {
        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era Era { get; set; } = new() { EraName = string.Empty };
    }
}
