using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase.BodyOfWaterDatabase;
using Sammlerplattform.Models.PlaceDatabase.BuildingDatabase;
using Sammlerplattform.Models.PlaceDatabase.FieldDatabase;
using Sammlerplattform.Models.PlaceDatabase.RegionDatabase;
using Sammlerplattform.Models.PlaceDatabase.ReliefDatabase;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using System.ComponentModel;
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

        [Display(Name = "Toponymie-Typ")]
        public int ToponymyTypeInt { get; set; }
        [NotMapped]
        public ToponymyType ToponymyTypeEnum
        {
            get => (ToponymyType)ToponymyTypeInt;
            set => ToponymyTypeInt = (int)value;
        }

        public int? ParentPlaceID { get; set; }
        public Place? ParentPlace { get; set; }
        public List<Place> ChildPlaceList { get; set; } = [];
        public Settlement? RelatedSettlement { get; set; }
        public BodyOfWater? BodyOfWater { get; set; }
        public Building? Building { get; set; }
        public Field? Field { get; set; }
        public Region? Region { get; set; }
        public Relief? Relief { get; set; }
        public Settlement? Settlement { get; set; }
        public TransportRoute? TransportRoute { get; set; }
        public List<Party> PartyList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
    }
    public enum ToponymyType
    {
        [Description("Flur")]
        Agronym = 0,
        [Description("Raum")]
        Choronym = 1,
        [Description("Weg")]
        Dromonym = 2,
        [Description("Gewässer")]
        Hydronym = 3,
        [Description("Siedlung")]
        Oeconym = 4,
        [Description("Gebäude")]
        Oecodonym = 5,
        [Description("Relief")]
        Oronym = 6
    }
}
