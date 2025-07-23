using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class Material
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int MaterialID { get; set; }
        public required string Name { get; set; }
        public List<ProductNMaterial> ProductNMaterialList { get; set; } = [];
    }
}
