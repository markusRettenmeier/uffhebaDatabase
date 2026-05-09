using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionArea
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionAreaID { get; set; }

        [NotMapped]
        [Display(Name = "CollectionAreaName", ResourceType = typeof(SharedResources))]
        public string CollectionAreaName { get; set; } = string.Empty;

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        // hier muss Concept, statt ConceptListView, da sonst Include nicht klappt
        public List<Concept> ConceptList { get; set; } = [];
        public List<StatePreservation> StatePreservationList { get; set; } = [];
    }
}
