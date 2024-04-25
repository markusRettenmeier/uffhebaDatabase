using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class Postmark
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Postmark_ID { get; set; }

        [Display(Name = "Betriebsarten")]
        public byte? PunchMode { get; set; }

        [Display(Name = "Stempellänge")]
        public int? PostmarkHeight { get; set; }

        [Display(Name = "Stempelhöhe")]
        public int? PostmarkWidth { get; set; }

        [Display(Name = "Krümmungsradius")]
        public int? CurvatureRadius { get; set; }

        [Display(Name = "Stempelformen")]
        public byte? PostmarkType { get; set; }

        [Display(Name = "Ort")]
        public int? City_ID { get; set; }

        [Display(Name = "Stempeldatum")]
        public DateTime? PostmarkDate { get; set; }

        [Display(Name = "Qualitätsstufen-Abstempelung")]
        public byte? PostmarkQuality { get; set; }

        [Display(Name = "Stempeltext")]
        public string? Text { get; set; }
    }
}
