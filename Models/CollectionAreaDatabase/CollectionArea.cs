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
        public int CollectionAreaID { get; set; }

        [NotMapped]
        [Required(ErrorMessageResourceName = "CollectionArea_Name_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public string CollectionAreaName { get; set; } = string.Empty;

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public List<Concept> ConceptList { get; set; } = [];
        public List<StatePreservation> StatePreservationList { get; set; } = [];
    }
}
