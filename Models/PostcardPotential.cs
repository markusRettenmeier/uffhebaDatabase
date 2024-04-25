using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class PostcardPotential : Product
    {
        [Display(Name = "Orte")]
        public List<City> CityList { get; set; } = [];

        [Display(Name = "Dienstsache")]
        public bool OfficialBusiness { get; set; }

        [Display(Name = "Wellrand")]
        public bool CorrugatedEdge { get; set; }

        [Display(Name = "Feldpost")]
        public bool Fieldpost { get; set; }

        [Display(Name = "Format")]
        public int? Formats { get; set; }

        [Display(Name = "Kartenart")]
        public int? CardType { get; set; }

        [Display(Name = "Kartenserie")]
        public int? CardSeries { get; set; }
        public bool Leporello { get; set; }
        public int? PostcardImprint_ID { get; set; }

        //[Display(Name = "Abrisspostkarte")]
        //public bool TearOffPostcard { get; set; }

        //[Display(Name = "Aufstellkarte")]
        //public bool SetUpPostcard { get; set; }
        public bool Propaganda { get; set; }

        [Display(Name = "Schmuckkarte")]
        public bool Ornament { get; set; }
    }
}