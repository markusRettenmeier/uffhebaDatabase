using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase
{
    public class StatePreservationCreateDTO
    {
        [Display(Name = "StatePreservation", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_StatePreservationName_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "SortingOrder", ResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_SortingOrder_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int SortingOrder { get; set; } = 1;

        public int CollectionAreaID { get; set; }
    }
}
