using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    // TPC-Inheritance
    public class ProductEntity
    {
        [Display(Name = "Ablageort")]
        public string? FilingLocation { get; set; }
        public string? Charge { get; set; }

        [Display(Name = "Preis")]
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        [Precision(18, 2)]
        public decimal? Price { get; set; }

        [Display(Name = "Fälschung")]
        public bool Fake { get; set; }
        public int? Material { get; set; }
        public string UsingIdentityUsers_ID { get; set; } = null!;
    }
}