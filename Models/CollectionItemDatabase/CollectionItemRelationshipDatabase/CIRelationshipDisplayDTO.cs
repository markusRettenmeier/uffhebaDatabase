using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase
{
    public class CIRelationshipDisplayDTO
    {
        public int Id { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resources.SharedResources))]
        public string CollectionItemRelationshipName { get; set; } = null!;

        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
}
