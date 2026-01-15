using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings
{
    public class ChangeEMailModel
    {
        [Required(ErrorMessageResourceName = "Error_Email_OldMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = "Error_Email_NotValid", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "OldEmail", ResourceType = typeof(SharedResources))]
        public string OldEmail { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Error_Email_NewMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = "Error_Email_NotValid", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "NewEmail", ResourceType = typeof(SharedResources))]
        public string NewEmail { get; set; } = string.Empty;

        [Display(Name = "NewEmail", ResourceType = typeof(SharedResources))]
        public bool IsEmailConfirmed { get; set; }

        [Required(ErrorMessageResourceName = "Password_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string? Password { get; set; }
        public bool HasPassword { get; set; }
    }
}
