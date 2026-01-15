using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings;
public class UsingIdentityUser : IdentityUser
{
    [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }
    [Required(ErrorMessageResourceName = "Error_Email_Missing", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Email", ResourceType = typeof(SharedResources))]
    public override string? Email { get => base.Email; set => base.Email = value; }
    [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
    public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    public List<Topic> TopicList { get; set; } = [];
    public List<TopicVote> TopicVoteList { get; set; } = [];
}

