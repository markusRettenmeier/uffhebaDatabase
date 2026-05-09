using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemNParticipant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemNParticipantID { get; set; }
        public int? CollectionItemEntityID { get; set; }
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        public int ParticipantID { get; set; }
        public Participant Participant { get; set; } = null!;
        public int RelationTypeId { get; set; }
        public CollectionItemRelationship RelationType { get; set; } = null!;
    }
}