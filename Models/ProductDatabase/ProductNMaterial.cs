using Sammlerplattform.Models.BrickDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductNMaterial
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductNMaterialID { get; set; }
        public int BrickEntityID { get; set; }
        public BrickEntity BrickEntity { get; set; } = null!;
        public int MaterialID { get; set; }
        public Material Material { get; set; } = null!;
        public bool IsPrimaryMaterial { get; set; } = false;
    }
}
