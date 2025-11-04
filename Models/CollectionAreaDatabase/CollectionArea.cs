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
        public int CollectionAreaID { get; set; }
        public required string CollectionAreaName { get; set; }
        public List<CollectionAttribute> CollectionAttributeList { get; set; } = [];
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public List<Concept> ConceptList { get; set; } = [];
        public List<State> StateList { get; set; } = [];
    }
}
