using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.ObjectLayerDatabase
{
    public class ObjectLayer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ObjectLayerID { get; set; }
        //public Geometry LayerGeometry { get; set; } = null!;
        public int ZIndex { get; set; }
        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;
    }
}
