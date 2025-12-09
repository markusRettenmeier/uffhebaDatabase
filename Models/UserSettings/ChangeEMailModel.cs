using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.UserSettings
{
    public class ChangeEMailModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "OldEmail", ResourceType = typeof(SharedResources))]
        public string OldEmail { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "NewEmail", ResourceType = typeof(SharedResources))]
        public string NewEmail { get; set; } = string.Empty;

        [Display(Name = "NewEmail", ResourceType = typeof(SharedResources))]
        public bool IsEmailConfirmed { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string? Password { get; set; }
        public bool HasPassword { get; set; }
    }
}
