using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class Place
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PlaceID { get; set; }
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
        public ICollection<PlaceNPlace> ConnectionsAsFirst { get; set; } = [];
        public ICollection<PlaceNPlace> ConnectionsAsSecond { get; set; } = [];

        [NotMapped]
        public IEnumerable<Place> ConnectedPlaces =>
            ConnectionsAsFirst.Select(c => c.Place2)
            .Concat(ConnectionsAsSecond.Select(c => c.Place1));
        public string? FurtherSpecs { get; set; }
        public string? WikipediaUrl { get; set; }
        public List<ParticipantNPlace> ParticipantNPlaceList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
}