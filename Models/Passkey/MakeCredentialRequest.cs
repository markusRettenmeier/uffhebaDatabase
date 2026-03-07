using Fido2NetLib;

namespace Sammlerplattform.Models.Passkey
{
    public class MakeCredentialRequest
    {
        public string SessionKey { get; set; } = string.Empty;
        public AuthenticatorAttestationRawResponse AttestationResponse { get; set; } = null!;
    }
}
