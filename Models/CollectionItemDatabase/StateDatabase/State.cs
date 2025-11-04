using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.StateDatabase
{
    public class State
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StateID { get; set; }
        public required string StateName { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public int? CollectionAreaID { get; set; }
        public CollectionAreaDatabase.CollectionArea? CollectionArea { get; set; }
        public int SortingOrder { get; set; }
        public bool IsGeneralState { get; set; }
    }
}
