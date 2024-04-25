using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class ChangeEMailModel
    {
        [Required(ErrorMessage = "Feld bitte ausfüllen")]
        [EmailAddress(ErrorMessage = "Dies ist keine E-Mail Adresse")]
        [Display(Name = "Alte E-Mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Feld bitte ausfüllen")]
        [EmailAddress(ErrorMessage = "Dies ist keine E-Mail Adresse")]
        [Display(Name = "Neue E-Mail")]
        public string NewEmail { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        [Required(ErrorMessage = "Feld bitte ausfüllen")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string? Password { get; set; }
    }
}
