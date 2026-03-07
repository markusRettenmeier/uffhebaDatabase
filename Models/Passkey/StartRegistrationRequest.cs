namespace Sammlerplattform.Models.Passkey
{
    public class StartRegistrationRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
