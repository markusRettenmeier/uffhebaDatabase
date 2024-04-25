using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Passwort bestätigen")]
        [Compare("Password", ErrorMessage = "Die PAsswörter stimmen nicht überein")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
