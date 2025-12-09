using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionAttributeID", ResourceType = typeof(SharedResources))]
        public int CollectionAttributeID { get; set; }

        [Required]
        [Display(Name = "CollectionAttributeName", ResourceType = typeof(SharedResources))]
        public required string CollectionAttributeName { get; set; }

        [Display(Name = "CollectionAttributeType", ResourceType = typeof(SharedResources))]
        public int CollectionAttributeTypeInt { get; set; }

        [NotMapped]
        [Display(Name = "CollectionAttributeType", ResourceType = typeof(SharedResources))]
        public CollectionAttributeType CollectionAttributeType
        {
            get => (CollectionAttributeType)CollectionAttributeTypeInt;
            set => CollectionAttributeTypeInt = (int)value;
        }

        [Display(Name = "RequiredAttribute", ResourceType = typeof(SharedResources))]
        public bool RequiredAttribute { get; set; }

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

        [Display(Name = "CollectionArea", ResourceType = typeof(SharedResources))]
        public CollectionArea CollectionArea { get; set; } = null!;

        [Display(Name = "CollectionAttributeValueList", ResourceType = typeof(SharedResources))]
        public List<CollectionAttributeValue> CollectionAttributeValueList { get; set; } = [];
    }

    public enum CollectionAttributeType
    {
        [Display(Name = "Text", ResourceType = typeof(SharedResources))]
        Text = 0,
        [Display(Name = "Number", ResourceType = typeof(SharedResources))]
        Number = 1,
        [Display(Name = "Date", ResourceType = typeof(SharedResources))]
        Date = 2,
        [Display(Name = "Decimal", ResourceType = typeof(SharedResources))]
        Decimal = 3,
        [Display(Name = "Bool", ResourceType = typeof(SharedResources))]
        Bool = 4
    }
}
