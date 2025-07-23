using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.PostcardDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductNColorVariant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductNColorVariantID { get; set; }
        public int BrickEntityID { get; set; }
        public BrickEntity BrickEntity { get; set; } = null!;
        public int? PostcardEntity_ID { get; set; }
        public PostcardEntity? PostcardEntity { get; set; }
        public int ColorID { get; set; }
        public Color Color { get; set; } = null!;
        public string? Note { get; set; }
        public bool IsPrimaryColor { get; set; } = false;
    }
}
