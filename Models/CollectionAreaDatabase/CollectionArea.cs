using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StateDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionArea
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

        [Required]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public required string CollectionAreaName { get; set; }

        [Display(Name = "CollectionAttributeList", ResourceType = typeof(SharedResources))]
        public List<CollectionAttribute> CollectionAttributeList { get; set; } = [];

        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

        [Display(Name = "ConceptList", ResourceType = typeof(SharedResources))]
        public List<Concept> ConceptList { get; set; } = [];

        [Display(Name = "StateList", ResourceType = typeof(SharedResources))]
        public List<State> StateList { get; set; } = [];
    }
}
