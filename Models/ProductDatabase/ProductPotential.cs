using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ProductDatabase
{
    //TPC-Inheritance
    public abstract class ProductPotential
    {
        public bool Immaterial { get; set; }

        [Display(Name = "Seriennummer")]
        [StringLength(50)]
        public string? SerialNumber { get; set; }
    }
}