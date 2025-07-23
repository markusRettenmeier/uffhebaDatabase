using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ImageDatabase
{
    public class Image
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Image_ID { get; set; }

        [Display(Name = "Künstler/Autor")]
        public int? ArtistAuthor_ID { get; set; }

        [Display(Name = "Höhe")]
        public double Height { get; set; } = 0;

        [Display(Name = "Breite")]
        public double Width { get; set; } = 0;

        [Display(Name = "Farbverarbeitung")]
        public int? ColorProcessing { get; set; }

        [Display(Name = "Bildfarbe")]
        public int? ColorImage_ID { get; set; }

        [Display(Name = "Jahr des Bildes")]
        public int? ImageYear { get; set; }

        [Display(Name = "Authentifizierung")]
        public int? ImageAuthentication { get; set; }

        [Display(Name = "Gerahmt")]
        public bool Framed { get; set; } = false;

        [Display(Name = "Ära")]
        public int? Era_ID { get; set; }

        [Display(Name = "Dimensionen")]
        public int Dimensions { get; set; } = 2;

        [Display(Name = "Bildwahrnehmung")]
        public int? ImagePerception { get; set; }

        [Display(Name = "Rand-Passepartout")]
        public bool Passepartout { get; set; }

        // Zu Stichworte
        [Display(Name = "Luftbild")]
        public bool Aerial { get; set; }

        [Display(Name = "Vollbild")]
        public bool FullScreen { get; set; }
    }
}
