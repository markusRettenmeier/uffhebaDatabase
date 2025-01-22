using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ManufacturingDate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ManufacturingDate_ID { get; set; }
        [Display(Name = "Exaktes Jahr")]
        public int? ExactYear { get; set; }
        [Display(Name = "Startjahr")]
        public int? StartYear { get; set; }
        [Display(Name = "Endjahr")]
        public int? EndYear { get; set; }
        [Display(Name = "Ist es geschätzt?")]
        public bool IsApproximate { get; set; }
        [Display(Name = "Notiz")]
        public string? Note { get; set; }
        public ICollection<PostcardEntity> PostcardEntityICollection { get; set; } = [];
        public ICollection<BrickEntity> BrickEntityICollection { get; set; } = [];

        public string? ValidateYears()
        {
            int earlestDate = -50000;
            if (ExactYear != null && (ExactYear < earlestDate || ExactYear > DateTime.Now.Year))
            {
                return "ExactYear must be between -4000 and the current year.";
            }

            if (StartYear != null && (StartYear < earlestDate || StartYear > DateTime.Now.Year))
            {
                return "StartYear must be between -4000 and the current year.";
            }

            return EndYear != null && (EndYear < earlestDate || EndYear > DateTime.Now.Year)
                ? "EndYear must be between -4000 and the current year."
                : StartYear != null && EndYear != null && StartYear > EndYear ? "StartYear must be less than or equal to EndYear." : null;
        }
    }
}
