using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ImprovementSuggestions
{
    public class TopicEditDto : TopicCreateDto
    {
        [Required(ErrorMessageResourceName = "Error_TopicId_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_TopicId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }
    }
}
