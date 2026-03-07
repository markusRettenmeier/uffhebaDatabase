using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase
{
    public class CollectionSet // Instead of Set, because of double meaning in RDBMS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionSetId", ResourceType = typeof(SharedResources))]
        public int CollectionSetId { get; set; }

        [Required(ErrorMessageResourceName = "CollectionSetName_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [NotMapped]
        [Display(Name = "CollectionSetName", ResourceType = typeof(SharedResources))]
        public required string CollectionSetName { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    }
}
