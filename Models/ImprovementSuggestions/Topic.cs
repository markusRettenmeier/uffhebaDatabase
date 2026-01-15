using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ImprovementSuggestions
{
    public class Topic
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Display(Name ="Title", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "TitleRequired", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Title { get; set; }

        [Display(Name = "Content", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "ContentRequired", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Content { get; set; }
        public required string UserId { get; set; }
        public UsingIdentityUser Author { get; set; } = null!;
        [Display(Name = "CreatedAt", ResourceType = typeof(SharedResources))]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Display(Name = "UpdatedAt", ResourceType = typeof(SharedResources))]
        public DateTime? UpdatedAt { get; set; }
        public List<TopicVote> VoteList { get; set; } = [];
    }
}
