using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-Mail wird benötigt")]
        [EmailAddress]
        [Display(Name = "E-Mail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Passwort wird benötigt")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string? Password { get; set; }

        //[Display(Name = "Erinnere mich?")]
        //public bool RememberMe { get; set; }
    }
}
