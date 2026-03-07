using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.Passkey;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings;

public class UsingIdentityUser : IdentityUser
{
    [PersonalData]
    [Required(ErrorMessageResourceName = "Error_DisplayName_Missing", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "DisplayName", ResourceType = typeof(SharedResources))]
    public required string DisplayName { get; set; }

    public virtual List<FidoCredential> FidoCredentialList { get; set; } = [];
    public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    public List<Topic> TopicList { get; set; } = [];
    public List<TopicVote> TopicVoteList { get; set; } = [];
}

