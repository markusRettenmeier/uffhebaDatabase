using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    //TPC-Inheritance
    public abstract class ProductPotential
    {
        public bool Immaterial { get; set; }

        [Display(Name = "Seriennummer")]
        [StringLength(50)]
        public string? SerialNumber { get; set; }

        //[StringLength(13)]
        //[Display(Name = "ISBN-Nummer")]
        //public string? ISBN { get; set; }
    }
}