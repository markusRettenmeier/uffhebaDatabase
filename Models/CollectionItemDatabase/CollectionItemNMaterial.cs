using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNMaterial
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemNMaterialID", ResourceType = typeof(SharedResources))]
        public int CollectionItemNMaterialID { get; set; }
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int CollectionItemEntityID { get; set; }
        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
        [Display(Name = "MaterialID", ResourceType = typeof(SharedResources))]
        public int MaterialID { get; set; }
        [Display(Name = "Material", ResourceType = typeof(SharedResources))]
        public Material Material { get; set; } = null!;
        [Display(Name = "IsPrimaryMaterial", ResourceType = typeof(SharedResources))]
        public bool IsPrimaryMaterial { get; set; } = false;
    }
}