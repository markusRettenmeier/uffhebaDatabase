using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class Place
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "PlaceNToponymyList", ResourceType = typeof(SharedResources))]
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];

        [Display(Name = "ToponymyType", ResourceType = typeof(SharedResources))]
        public int ToponymyTypeInt { get; set; }
        [NotMapped]
        [Display(Name = "ToponymyType", ResourceType = typeof(SharedResources))]
        public ToponymyType ToponymyTypeEnum
        {
            get => (ToponymyType)ToponymyTypeInt;
            set => ToponymyTypeInt = (int)value;
        }

        [Display(Name = "ParentPlaceID", ResourceType = typeof(SharedResources))]
        public int? ParentPlaceID { get; set; }
        [Display(Name = "ParentPlace", ResourceType = typeof(SharedResources))]
        public Place? ParentPlace { get; set; }
        [Display(Name = "ChildPlaceList", ResourceType = typeof(SharedResources))]
        public List<Place> ChildPlaceList { get; set; } = [];
        [Display(Name = "RelatedSettlement", ResourceType = typeof(SharedResources))]
        public Settlement? RelatedSettlement { get; set; }
        [Display(Name = "BodyOfWater", ResourceType = typeof(SharedResources))]
        public BodyOfWater? BodyOfWater { get; set; }
        [Display(Name = "Building", ResourceType = typeof(SharedResources))]
        public Building? Building { get; set; }
        [Display(Name = "Field", ResourceType = typeof(SharedResources))]
        public Field? Field { get; set; }
        [Display(Name = "Region", ResourceType = typeof(SharedResources))]
        public Region? Region { get; set; }
        [Display(Name = "Relief", ResourceType = typeof(SharedResources))]
        public Relief? Relief { get; set; }
        [Display(Name = "Settlement", ResourceType = typeof(SharedResources))]
        public Settlement? Settlement { get; set; }
        [Display(Name = "TransportRoute", ResourceType = typeof(SharedResources))]
        public TransportRoute? TransportRoute { get; set; }
        [Display(Name = "PartyList", ResourceType = typeof(SharedResources))]
        public List<Party> PartyList { get; set; } = [];
        [Display(Name = "CollectionItemNPlaceList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
    public enum ToponymyType
    {
        [Display(Name = "Field", ResourceType = typeof(SharedResources))]
        Agronym = 0,
        [Display(Name = "Region", ResourceType = typeof(SharedResources))]
        Choronym = 1,
        [Display(Name = "TransportRoute", ResourceType = typeof(SharedResources))]
        Dromonym = 2,
        [Display(Name = "BodyOfWater", ResourceType = typeof(SharedResources))]
        Hydronym = 3,
        [Display(Name = "Settlement", ResourceType = typeof(SharedResources))]
        Oeconym = 4,
        [Display(Name = "Building", ResourceType = typeof(SharedResources))]
        Oecodonym = 5,
        [Display(Name = "Relief", ResourceType = typeof(SharedResources))]
        Oronym = 6
    }
}
