using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.PostcardDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductPictureDatabase
{
    public class ProductPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductPictureID { get; set; }
        public string FileExtension { get; set; } = "png";

        [NotMapped]
        public bool Frontside => PerspectiveInt == 0;
        
        [Display(Name = "Perspektive")]
        [NotMapped]
        public PerspectiveType Perspective
        {
            get => PerspectiveInt == null ? PerspectiveType.Vorderseite : (PerspectiveType)PerspectiveInt; set => PerspectiveInt = (int)value;
        }
        public int? PerspectiveInt { get; set; }
        public int? PostcardEntityID { get; set; }
        public PostcardEntity? PostcardEntity { get; set; }
        public int? BrickEntityID { get; set; }
        public BrickEntity? BrickEntity { get; set; }
        public string? TextPositionJson { get; set; }
        [NotMapped]
        public IFormFile? Datei { get; set; }
    }
    public enum PerspectiveType
    {
        Vorderseite = 0,
        Rückseite = 1,
        Linke_Seite = 2,
        Rechte_Seite = 3
    }
}