using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase
{
    public class StatePreservation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "StatePreservationID", ResourceType = typeof(SharedResources))]
        public int StatePreservationID { get; set; }

        [NotMapped]
        [Display(Name = "StatePreservationName", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "StatePreservation_Name_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public required string StatePreservationName { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public int? CollectionAreaID { get; set; }
        public CollectionArea? CollectionArea { get; set; }

        [Display(Name = "SortingOrder", ResourceType = typeof(SharedResources))]
        public int SortingOrder { get; set; }

        [Display(Name = "IsGeneralState", ResourceType = typeof(SharedResources))]
        public bool IsGeneralState { get; set; }
    }
}
