using Microsoft.AspNetCore.Authorization;

namespace Sammlerplattform.Services.Passkey
{
    public class RecentPasskeyHandler : AuthorizationHandler<RecentPasskeyRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RecentPasskeyRequirement requirement)
        {
            var timestampClaim = context.User.FindFirst("passkey_reverified_at");

            if (timestampClaim == null)
                return Task.CompletedTask;

            if (!long.TryParse(timestampClaim.Value, out var timestamp))
                return Task.CompletedTask;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (now - timestamp <= requirement.ValidForSeconds)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class RecentPasskeyRequirement(int validForSeconds) : IAuthorizationRequirement
    {
        public int ValidForSeconds { get; } = validForSeconds;
    }
}
