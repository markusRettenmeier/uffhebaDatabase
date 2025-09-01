using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionDatabase
{
    public class CollectionField
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionFieldID { get; set; }
        public string CollectionFieldName { get; set; }
        public CollectionFieldType CollectionFieldType { get; set; }
        public bool required { get; set; }
        public int CollectionID { get; set; }
        public Collection Collection { get; set; } = null!;
    }

    public enum CollectionFieldType
    {
        Text = 0,
        Number = 1,
        Date = 2,
        Decimal = 3
    }
}
