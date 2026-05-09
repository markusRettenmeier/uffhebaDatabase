using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase
{
    public class CollectionItemRelationship
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollectionItemRelationshipId { get; set; }

        [NotMapped]
        [Display(Name = "Name", ResourceType = typeof(Resources.SharedResources))]
        public string CollectionItemRelationshipName { get; set; } = null!;

        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
}
