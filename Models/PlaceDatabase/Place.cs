using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Resources;
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

        [Display(Name = "FurtherSpecs", ResourceType = typeof(SharedResources))]
        public string? FurtherSpecs { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        //public List<Party> PartyList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
}