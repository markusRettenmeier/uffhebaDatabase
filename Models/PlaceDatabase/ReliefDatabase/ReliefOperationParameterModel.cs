using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.ReliefDatabase
{
    public class ReliefOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "Relief", ResourceType = typeof(SharedResources))]
        public Relief Relief { get; set; } = new();
    }
}
