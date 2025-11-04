using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class Material
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int MaterialID { get; set; }
        public required string MaterialName { get; set; }
        public List<CollectionItemNMaterial> CollectionItemNMaterialList { get; set; } = [];
    }
}