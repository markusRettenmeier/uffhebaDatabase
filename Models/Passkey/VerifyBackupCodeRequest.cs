using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Passkey
{
    public class VerifyBackupCodeRequest
    {
        [Display(Name = "BackupCode", ResourceType = typeof(SharedResources))]
        public required string BackupCode { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
        public required string Username { get; set; }
    }
}
