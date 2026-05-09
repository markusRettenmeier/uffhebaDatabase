using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.EraDatabase
{
    public class EraCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_Era_NameMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "EraName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
    }
}
