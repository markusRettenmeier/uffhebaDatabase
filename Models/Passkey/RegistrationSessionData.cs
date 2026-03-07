using Fido2NetLib;

namespace Sammlerplattform.Models.Passkey
{
    public class RegistrationSessionData
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public CredentialCreateOptions Options { get; set; } = null!;
    }
}
