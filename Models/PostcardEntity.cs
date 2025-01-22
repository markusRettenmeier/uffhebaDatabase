using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models
{
    public class PostcardEntity : ProductEntity<PostcardConditionType>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ID der Postkarte")]
        public int PostcardEntity_ID { get; set; }

        [Display(Name = "Belegnummer")]
        public int? PostcardPotential_ID { get; set; }

        [Display(Name = "Absender")]
        public int? Sender_ID { get; set; }

        [Display(Name = "Empfänger")]
        public int? Receiver_ID { get; set; }
        public int? Stamp_ID { get; set; }

        [Display(Name = "Datum im Text")]
        public DateTime? DateInText { get; set; }
        public int? SenderPostmark_ID { get; set; }
        public int? RecipientPostmark_ID { get; set; }
        public string? Text { get; set; }

        [Display(Name = "Farbe RAL-Schrift Vorderseite")]
        public int? ColorRALWritingFrontside { get; set; }

        [Display(Name = "Farbe RAL-Druck Rückseite")]
        public int? ColorRALPrintingBackside { get; set; }
        //[Display(Name = "Erhaltungszustand")]
        //[NotMapped]
        //public PostcardConditionType? ConditionEnum { get; set; }
        //public int ConditionInt { get; set; }
    }

    public enum PostcardConditionType
    {
        [Description("Keine Angabe")]
        KeineAngabe = 0,
        [Description("Ug, ungebraucht")]
        Ug = 1,
        [Description("Ugbs, beschrieben, jedoch nicht gelaufen")]
        Ugbs = 2,
        [Description("Gbmm, gelaufen mit Marke")]
        Gbmm = 3,
        [Description("Gbom, gelaufen, jedoch ohne Marke, bzw. Marke entfernt")]
        Gbom = 4,
        [Description("R, repariert")]
        R = 5,
    }
}
