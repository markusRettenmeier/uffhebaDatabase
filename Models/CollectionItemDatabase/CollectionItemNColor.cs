using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNColor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemNColorID { get; set; }
        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
        public int ColorID { get; set; }
        public Color Color { get; set; } = null!;
        public string? Note { get; set; }
        public bool IsPrimaryColor { get; set; } = false;
    }
}
