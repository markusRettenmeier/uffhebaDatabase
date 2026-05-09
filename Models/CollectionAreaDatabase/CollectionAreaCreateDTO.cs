using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAreaCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_CollectionAreaName_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
    }
}
