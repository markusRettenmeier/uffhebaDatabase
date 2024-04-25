using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class PostcardImprint : Graphics
    {
        //Drei-Bild-AK, Vier-Bild-AK, Mehrbild-AK
        [Display(Name = "Bilderanzahl")]
        public string? PictureCount { get; set; }

        [Display(Name = "Sammelgebiet")]
        public string? CollectionArea { get; set; }

        [Display(Name = "Gebäude")]
        public string? Buildings { get; set; }
    }
}