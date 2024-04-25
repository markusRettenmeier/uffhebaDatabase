using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class RegisterViewModel : IdentityUser
    {
        [Required(ErrorMessage = "E-Mail ist notwendig")]
        [EmailAddress(ErrorMessage = "E-Mail ist ungültig")]
        [Display(Name = "E-Mail*")]
        public override string? Email { get => base.Email; set => base.Email = value; }

        [Required(ErrorMessage = "Benutzername ist notwendig")]
        [Display(Name = "Benutzername*")]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [Required(ErrorMessage = "Passwort ist notwendig")]
        [StringLength(100, ErrorMessage = "Das {0} muss zumindest {2} und maximal {1} Zeichen lang sein.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort*")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Bitte bestätige dein Passwort")]
        [DataType(DataType.Password)]
        [Display(Name = "Bestätige Passwort*")]
        [Compare("Password", ErrorMessage = "Das Passwort und Bestätigungspasswort stimmen nicht überein.")]
        public string? ConfirmPassword { get; set; }
    }
}
