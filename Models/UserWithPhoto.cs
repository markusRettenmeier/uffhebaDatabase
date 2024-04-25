using Microsoft.EntityFrameworkCore;

namespace Sammlerplattform.Models
{
    [Keyless]
    public class UserWithPhoto
    {
        public UsingIdentityUser UsingIdentityUsers { get; set; } = new UsingIdentityUser();
        public byte[] UserPictured { get; set; } = null!;
    }
}
