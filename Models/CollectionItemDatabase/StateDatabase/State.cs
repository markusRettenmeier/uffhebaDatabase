using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.StateDatabase
{
    public class State
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "StateID", ResourceType = typeof(SharedResources))]
        public int StateID { get; set; }
        [Display(Name = "StateName", ResourceType = typeof(SharedResources))]
        public required string StateName { get; set; }
        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public int? CollectionAreaID { get; set; }
        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public CollectionArea? CollectionArea { get; set; }
        [Display(Name = "SortingOrder", ResourceType = typeof(SharedResources))]
        public int SortingOrder { get; set; }
        [Display(Name = "IsGeneralState", ResourceType = typeof(SharedResources))]
        public bool IsGeneralState { get; set; }
    }
}
