using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.UserSettings
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessageResourceName = "Password_Old_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Display(Name = "OldPassword", ResourceType = typeof(SharedResources))]
        public string? OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "Password_Old_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(SharedResources))]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword { get; set; }
    }
}
