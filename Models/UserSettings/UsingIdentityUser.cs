using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings;
public class UsingIdentityUser : IdentityUser
{
    [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }
    [Required]
    [Display(Name = "Email", ResourceType = typeof(SharedResources))]
    public override string? Email { get => base.Email; set => base.Email = value; }
    [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
    public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
}

