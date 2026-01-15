using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionArea
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

        [NotMapped]
        [Required(ErrorMessageResourceName = "CollectionArea_Name_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public string CollectionAreaName { get; set; } = string.Empty;

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

        [Display(Name = "ConceptList", ResourceType = typeof(SharedResources))]
        public List<Concept> ConceptList { get; set; } = [];

        [Display(Name = "StatePreservationList", ResourceType = typeof(SharedResources))]
        public List<StatePreservation> StatePreservationList { get; set; } = [];
    }
}
