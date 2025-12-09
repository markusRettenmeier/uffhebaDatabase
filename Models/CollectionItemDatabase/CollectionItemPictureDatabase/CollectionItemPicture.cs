using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase
{
    public class CollectionItemPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemPictureID", ResourceType = typeof(SharedResources))]
        public int CollectionItemPictureID { get; set; }

        [NotMapped]
        [Display(Name = "Side_Front", ResourceType = typeof(SharedResources))]
        public bool Frontside => PerspectiveInt == 0;

        [Display(Name = "Perspective", ResourceType = typeof(SharedResources))]
        public int? PerspectiveInt { get; set; }
        [NotMapped]
        [Display(Name = "Perspective", ResourceType = typeof(SharedResources))]
        public PerspectiveType Perspective
        {
            get => PerspectiveInt == null ? PerspectiveType.Frontside : (PerspectiveType)PerspectiveInt; set => PerspectiveInt = (int)value;
        }

        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int CollectionItemEntityID { get; set; }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;

        [NotMapped]
        [Display(Name = "IFormFile", ResourceType = typeof(SharedResources))]
        public IFormFile? IFormFile { get; set; }
    }
    public enum PerspectiveType
    {
        [Display(Name = "Side_Front", ResourceType = typeof(SharedResources))]
        Frontside = 0,
        [Display(Name = "Side_Back", ResourceType = typeof(SharedResources))]
        BackSide = 1,
        [Display(Name = "Side_Left", ResourceType = typeof(SharedResources))]
        LeftSide = 2,
        [Display(Name = "Side_Right", ResourceType = typeof(SharedResources))]
        RightSide = 3,
        [Display(Name = "Side_Top", ResourceType = typeof(SharedResources))]
        TopSide = 4,
        [Display(Name = "Side_Bottom", ResourceType = typeof(SharedResources))]
        BottomSide = 5
    }
}