using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class Concept
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public required string ConceptName { get; set; }  // z. B. "Mönchziegel", "Klappstuhl"

        [Display(Name = "Description", ResourceType = typeof(SharedResources))]
        public string? Description { get; set; }

        [Display(Name = "IsGeneralConcept", ResourceType = typeof(SharedResources))]
        public bool IsGeneralConcept => CollectionAreaID == 0;

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int? CollectionAreaID { get; set; }

        [Display(Name = "CollectionArea", ResourceType = typeof(SharedResources))]
        public CollectionArea? CollectionArea { get; set; } = null!;

        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public int CollectionAttributeTypeInt { get; set; }

        [NotMapped]
        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public CollectionAttributeType CollectionAttributeType
        {
            get => (CollectionAttributeType)CollectionAttributeTypeInt;
            set => CollectionAttributeTypeInt = (int)value;
        }


        [Display(Name = "RequiredAttribute", ResourceType = typeof(SharedResources))]
        public bool RequiredConcept { get; set; }
    }
}
