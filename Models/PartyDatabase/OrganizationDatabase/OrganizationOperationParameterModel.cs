using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class OrganizationOperationParameterModel : PartyOperationParameterModel
    {
        [Display(Name = "Organization", ResourceType = typeof(SharedResources))]
        public Organization Organization { get; set; } = new();
    }
}
