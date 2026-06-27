using Sammlerplattform.Models.CollectionAreaDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase
{
    public class StatePreservation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StatePreservationID { get; set; }
        public int SortingOrder { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public int CollectionAreaID { get; set; }
        public CollectionArea CollectionArea { get; set; } = null!;
    }
}
