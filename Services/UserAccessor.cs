using System.Security.Claims;

namespace Sammlerplattform.Services
{
    public class UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        public string GetUserId()
        {
            var user = httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return userId ?? string.Empty;
        }
    }
}
