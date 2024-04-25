using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class AuthorArtist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuthorArtist_ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Künstler/Autor")]
        public string AAName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Künstlerbeschreibung")]
        public string? ArtistDescription { get; set; }

        [Display(Name = "Preise")]
        public int? Prizes { get; set; }

        [StringLength(30)]
        [Display(Name = "Berufsbezeichnung")]
        public string? Profession { get; set; }

        [StringLength(30)]
        [Display(Name = "Signatur")]
        public string? AASignature { get; set; }
    }
}
