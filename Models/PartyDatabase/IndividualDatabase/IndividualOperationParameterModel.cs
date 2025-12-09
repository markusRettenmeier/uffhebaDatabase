using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PartyDatabase.IndividualDatabase
{
    public class IndividualOperationParameterModel : PartyOperationParameterModel
    {
        [Display(Name = "Individual", ResourceType = typeof(SharedResources))]
        public Individual Individual { get; set; } = new();
    }
}
