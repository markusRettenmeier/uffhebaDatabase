using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNColor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemNColorID", ResourceType = typeof(SharedResources))]
        public int CollectionItemNColorID { get; set; }
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int CollectionItemEntityID { get; set; }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
        [Display(Name = "ColorID", ResourceType = typeof(SharedResources))]
        public int ColorID { get; set; }
        [Display(Name = "Color", ResourceType = typeof(SharedResources))]
        public Color Color { get; set; } = null!;

        [Display(Name = "IsPrimaryColor", ResourceType = typeof(SharedResources))]
        public bool IsPrimaryColor { get; set; } = false;
    }
}
