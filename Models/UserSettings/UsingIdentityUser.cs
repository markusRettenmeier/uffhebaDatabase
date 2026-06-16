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
    [Display(Name = "DisplayName", ResourceType = typeof(SharedResources))]
    public required string DisplayName { get; set; }

    public virtual List<FidoCredential> FidoCredentialList { get; set; } = [];
    public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    public List<Topic> TopicList { get; set; } = [];
    public List<TopicVote> TopicVoteList { get; set; } = [];
    public List<BackupCode> BackupCodeList { get; set; } = [];
}

public class BackupCode
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string HashedCode { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    //[ForeignKey("UserId")]
    public UsingIdentityUser User { get; set; } = null!;
}