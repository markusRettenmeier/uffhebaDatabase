using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceDisplayDTO
    {
        public int PlaceID { get; set; }
        public List<ToponymyDisplayDTO> ToponymyList { get; set; } = [];
        public ICollection<PlaceNPlace> ConnectionsAsFirst { get; set; } = [];
        public ICollection<PlaceNPlace> ConnectionsAsSecond { get; set; } = [];
        public List<PlaceDisplayDTO> ConnectedPlaces =>
            ConnectionsAsFirst.Select(c => new PlaceDisplayDTO
            {
                PlaceID = c.Place2.PlaceID,
                ToponymyList = c.Place2.PlaceNToponymyList.Select(x => new ToponymyDisplayDTO
                {
                    Id = x.ToponymyID,
                    Name = x.Toponymy.ToponymyName,
                    IsCurrentName = x.IsCurrentName
                }).ToList(),
                FurtherSpecs = c.Place2.FurtherSpecs,
                WikipediaUrl = c.Place2.WikipediaUrl
            })
            .Concat(ConnectionsAsSecond.Select(c => new PlaceDisplayDTO
            {
                PlaceID = c.Place1.PlaceID,
                ToponymyList = c.Place1.PlaceNToponymyList.Select(x => new ToponymyDisplayDTO
                {
                    Id = x.ToponymyID,
                    Name = x.Toponymy.ToponymyName,
                    IsCurrentName = x.IsCurrentName
                }).ToList(),
                FurtherSpecs = c.Place1.FurtherSpecs,
                WikipediaUrl = c.Place1.WikipediaUrl
            })).ToList();

        [Display(Name = "FurtherSpecs", ResourceType = typeof(SharedResources))]
        public string? FurtherSpecs { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }
        public List<ParticipantNPlace> ParticipantNPlaceList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }

    public class ToponymyDisplayDTO
    {
        public int PlaceNToponymyID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCurrentName { get; set; }
    }
}
