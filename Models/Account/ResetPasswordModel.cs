using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessageResourceName = "Error_Email_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = "Error_Email_NotValid", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public required string Email { get; set; }

        [Required(ErrorMessageResourceName = "Error_Email_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public required string Password { get; set; }

        [Required(ErrorMessageResourceName = "Error_Password_Empty", ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Code", ResourceType = typeof(SharedResources))]
        public required string Code { get; set; }
    }
}
