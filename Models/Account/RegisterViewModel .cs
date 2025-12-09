using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class RegisterViewModel : IdentityUser
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public override string? Email { get => base.Email; set => base.Email = value; }

        [Required]
        [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [Required]
        [StringLength(100, MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        public required string ConfirmPassword { get; set; }
    }
}
