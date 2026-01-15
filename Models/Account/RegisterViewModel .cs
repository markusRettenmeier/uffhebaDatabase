using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class RegisterViewModel : IdentityUser
    {
        [Required(ErrorMessageResourceName = "Error_Email_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = "Error_Email_NotValid", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public override string? Email { get => base.Email; set => base.Email = value; }

        [Required(ErrorMessageResourceName = "Error_User_NameEmpty", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [Required(ErrorMessageResourceName = "Error_Password_Empty", ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string? Password { get; set; }

        [Required(ErrorMessageResourceName = "Error_Password_Empty", ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        public required string ConfirmPassword { get; set; }
    }
}
