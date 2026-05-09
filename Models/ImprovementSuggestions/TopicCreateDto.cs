using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ImprovementSuggestions
{
    public class TopicCreateDto
    {
        [Display(Name = "Title", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "TitleRequired", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Title { get; set; }

        [Display(Name = "Content", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "ContentRequired", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Content { get; set; }
    }
}
