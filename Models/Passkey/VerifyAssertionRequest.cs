using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Sammlerplattform.Models.Passkey
{
    public class VerifyAssertionRequest
    {
        public AuthenticatorAssertionRawResponse Assertion { get; set; } = default!;
        public string SessionKey { get; set; } = default!;
    }    
}
