using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.Passkey
{
    public class FidoCredential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_UserID_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required string UserId { get; set; }

        [Required(ErrorMessageResourceName = "Error_CredentialId_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required byte[] CredentialId { get; set; }

        [Required(ErrorMessageResourceName = "Error_PublicKey_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required byte[] PublicKey { get; set; }

        public long SignatureCounter { get; set; }

        public string? CredType { get; set; }

        public DateTime RegDate { get; set; }

        public Guid AaGuid { get; set; }

        [MaxLength(200)]
        public string? DeviceName { get; set; }
        public UsingIdentityUser User { get; set; } = null!;
    }
}
