using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class PostcardEntity : ProductEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "Nummer der Postkarte")]
        public int PostcardEntity_ID { get; set; }

        [Display(Name = "Belegnummer")]
        public int? PostcardPotential_ID { get; set; }

        [Display(Name = "Absender")]
        public int? Sender_ID { get; set; }

        [Display(Name = "Empfänger")]
        public int? Receiver_ID { get; set; }

        public int? Stamp_ID { get; set; }

        [Display(Name = "Zustand")]
        public int? ConditionOfCard { get; set; }

        [Display(Name = "Datum im Text")]
        public DateTime? DateInText { get; set; }
        public int? SenderPostmark_ID { get; set; }
        public int? RecipientPostmark_ID { get; set; }
        public string? Text { get; set; }

        [Display(Name = "Farbe RAL-Schrift Vorderseite")]
        public int? ColorRALWritingFrontside { get; set; }

        [Display(Name = "Farbe RAL-Druck Rückseite")]
        public int? ColorRALPrintingBackside { get; set; }
    }
}
