using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class UserPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int UserPicture_ID { get; set; }

        [Display(Name = "Benutzerfoto")]
        [PersonalData]
        public byte[] Photo { get; set; } = null!;

        public string? UsingIdentityUsers_ID { get; set; }

        //[ForeignKey("UsingIdentityUsers_ID")]
        //public virtual UsingIdentityUser UsingIdentityUsers { get; set; } = new UsingIdentityUser(); 
    }
}
