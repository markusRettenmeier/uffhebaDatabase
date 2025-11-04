using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings;

// Add profile data for application users by adding properties to the UsingIdentityUser class
public class UsingIdentityUser : IdentityUser
{
    [Display(Name = "Benutzername")]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }

    [Required]
    public override string? Email { get => base.Email; set => base.Email = value; }

    public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
}

