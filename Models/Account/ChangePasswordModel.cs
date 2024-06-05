using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Feld bitte ausfüllen")]
        [DataType(DataType.Password)]
        [Display(Name = "Aktuelles Passwort")]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Feld bitte ausfüllen")]
        [StringLength(100, ErrorMessage = "Das Passwort muss mindestens 12 Zeichen haben.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Neues Passwort")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bestätige neues Passwort")]
        [Compare("NewPassword", ErrorMessage = "Das neue Passwort stimmt nicht überein.")]
        public string? ConfirmPassword { get; set; }
    }
}
