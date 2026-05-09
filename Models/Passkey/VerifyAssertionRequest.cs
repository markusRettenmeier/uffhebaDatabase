using Fido2NetLib;

namespace Sammlerplattform.Models.Passkey
{
    public class VerifyAssertionRequest
    {
        public AuthenticatorAssertionRawResponse Assertion { get; set; } = default!;
        public string SessionKey { get; set; } = default!;
    }
}
