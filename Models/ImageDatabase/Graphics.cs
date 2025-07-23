using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ImageDatabase
{
    //TPH-Inheritance
    public class Graphics : Image
    {
        //Foreign Key To Printing
        public int? Printing_ID { get; set; }

        [Display(Name = "Auflagengröße")]
        public int CirculationSize { get; set; } = 1;
        //public int? Photography_ID { get; set; }
        //public int? Drawing_ID { get; set; }
        //public int? Painting_ID { get; set; }
    }
}