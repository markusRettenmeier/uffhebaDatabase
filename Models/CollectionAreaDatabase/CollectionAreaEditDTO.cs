using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAreaEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_CollectionAreaId_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_CollectionAreaId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_CollectionAreaName_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
    }
}
