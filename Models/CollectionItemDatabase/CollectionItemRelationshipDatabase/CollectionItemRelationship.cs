using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase
{
    public class CollectionItemRelationship
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollectionItemRelationshipId { get; set; }
        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
}
