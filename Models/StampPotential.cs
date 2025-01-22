using System.ComponentModel.DataAnnotations;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models
{
    public class StampPotential : ProductPotential
    {
        [Display(Name = "Markenart")]
        public int? StampType { get; set; }

        [Display(Name = "Marke gedruckt")]
        public bool Printed { get; set; }

        [Display(Name = "Michel-Nr.")]
        public string? MiNr { get; set; }

        [Display(Name = "Wasserzeichen")]
        public bool Watermark { get; set; }

        [Display(Name = "Gummierung")]
        public int? Gumming { get; set; }
    }
}
