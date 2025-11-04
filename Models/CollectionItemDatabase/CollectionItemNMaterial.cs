using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNMaterial
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemNMaterialID { get; set; }
        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
        public int MaterialID { get; set; }
        public Material Material { get; set; } = null!;
        public bool IsPrimaryMaterial { get; set; } = false;
    }
}