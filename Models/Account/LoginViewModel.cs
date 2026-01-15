using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessageResourceName = "Error_Email_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = "Error_Email_NotValid", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string? Email { get; set; }

        [Required(ErrorMessageResourceName = "Error_Password_Empty", ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string? Password { get; set; }
    }
}
