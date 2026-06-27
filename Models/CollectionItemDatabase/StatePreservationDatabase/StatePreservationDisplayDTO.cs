using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase
{
    public class StatePreservationDisplayDTO
    {
        [Display(Name = "StatePreservationID", ResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Display(Name = "StatePreservation", ResourceType = typeof(SharedResources))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "SortingOrder", ResourceType = typeof(SharedResources))]
        public int SortingOrder { get; set; }   
        public static List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

        public int CollectionAreaID { get; set; }
        public bool IsDeletable { get; set; } = CollectionItemEntityList.Count == 0;
    }
}
