using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase
{
    public class CollectionItemPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemPictureID { get; set; }
        //public string FileExtension { get; set; } = "png";

        [NotMapped]
        public bool Frontside => PerspectiveInt == 0;

        [Display(Name = "Perspektive")]
        [NotMapped]
        public PerspectiveType Perspective
        {
            get => PerspectiveInt == null ? PerspectiveType.Vorderseite : (PerspectiveType)PerspectiveInt; set => PerspectiveInt = (int)value;
        }
        public int? PerspectiveInt { get; set; }
        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
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