using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models;

// Add profile data for application users by adding properties to the UsingIdentityUser class
public class UsingIdentityUser : IdentityUser
{
    [Display(Name = "Benutzername")]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }

    [Required]
    public override string? Email { get => base.Email; set => base.Email = value; }
    //[Display(Name = "Vorname")]
    //[PersonalData]
    //[Column(TypeName = "nvarchar(100)")]
    //public string? Firstname { get; set; }

    //[Display(Name ="Nachname")]
    //[PersonalData]
    //[Column(TypeName = "nvarchar(100)")]
    //public string? LastName { get; set; }

    //[Phone]
    //[Display(Name = "Telefonnummer")]
    //public override string? PhoneNumber { get; set; }

    public string? StripeCustomer_ID { get; set; }
}

