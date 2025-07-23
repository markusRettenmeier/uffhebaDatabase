using System.Security.Claims;

namespace Sammlerplattform.Services
{
    public class UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        public string GetUserId()
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            string? userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return userId ?? string.Empty;
        }
    }
}
