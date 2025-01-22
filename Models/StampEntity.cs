using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models
{
    public class StampEntity : ProductEntity<StampConditionType>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StampEntity_ID { get; set; }

        [Display(Name = "Briefmarken-Qualitätsstufen-Schnitt")]
        public int? StampQualityCutting { get; set; }

        [Display(Name = "Briefmarken-Qualitätsstufen-Durchstich")]
        public int? StampQualityPiercing { get; set; }

        [Display(Name = "Briefmarken-Qualitätsstufen-Zähnung")]
        public int? StampQualityPerforation { get; set; }

        [Display(Name = "Briefmarken-Qualitätsstufen-Zentrierung")]
        public int? StampQualityCentration { get; set; }

        [Display(Name = "Trennungsarten")]
        public int? SeparationType { get; set; }

        [Display(Name = "Zähnung")]
        public int? Perforation { get; set; }

        [Display(Name = "Entwertung")]
        public int? Cancellation { get; set; }
        public int StampPotential_ID { get; set; }

        [Display(Name = "StampColor")]
        public int? StampColor { get; set; }
        //[Display(Name = "Erhaltungszustand")]
        //[NotMapped]
        //public StampConditionType? ConditionEnum { get; set; }
        //public int ConditionInt { get; set; }
    }

    public enum StampConditionType
    {

    }
}
