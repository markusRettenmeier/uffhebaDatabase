using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionAttributeID { get; set; }
        public required string CollectionAttributeName { get; set; }
        public int CollectionAttributeTypeInt { get; set; }
        [NotMapped]
        public CollectionAttributeType CollectionAttributeType
        {
            get => (CollectionAttributeType)CollectionAttributeTypeInt;
            set => CollectionAttributeTypeInt = (int)value;
        }
        public bool Required { get; set; }
        public int CollectionAreaID { get; set; }
        public CollectionArea CollectionArea { get; set; } = null!;
        public List<CollectionItemValue> CollectionItemValueList { get; set; } = [];
    }

    public enum CollectionAttributeType
    {
        Text = 0,
        Number = 1,
        Date = 2,
        Decimal = 3,
        Bool = 4
    }
}
