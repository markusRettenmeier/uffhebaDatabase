using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.EraDatabase
{
    public class EraEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_Era_IdMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_EraId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_Era_NameMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "EraName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
    }
}
