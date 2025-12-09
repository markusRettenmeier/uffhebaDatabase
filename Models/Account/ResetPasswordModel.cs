using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Code", ResourceType = typeof(SharedResources))]
        public required string Code { get; set; }
    }
}
