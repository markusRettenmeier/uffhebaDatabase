using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class Material
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "MaterialID", ResourceType = typeof(SharedResources))]
        public int MaterialID { get; set; }
        [Display(Name = "MaterialName", ResourceType = typeof(SharedResources))]
        public required string MaterialName { get; set; }
        [Display(Name = "CollectionItemNMaterialList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNMaterial> CollectionItemNMaterialList { get; set; } = [];
    }
}