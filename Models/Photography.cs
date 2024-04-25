using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class Photography : Image
    {
        [Display(Name = "Fototechnik")]
        public int? Phototechnic { get; set; }

        [Display(Name = "Motiv")]
        public int? Motive { get; set; }
    }
}
