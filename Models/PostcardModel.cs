using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class PostcardModel
    {

        [Display(Name = "Gibt es ein Bild?")]
        public bool HasImage { get; set; } = false;
        [Display(Name = "Gibt es einen Sender?")]
        public bool HasSender { get; set; } = false;
        [Display(Name = "Gibt es einen Empfänger?")]
        public bool HasReceiver { get; set; } = false;
        public List<(City City, List<Postalcode> PostalcodeList, Geography Geography)> CityTupleList { get; set; } = [];
        public List<int> CityIDList { get; set; } = [];
        public string ColorRALWriting { get; set; } = string.Empty;
        public string ColorImage { get; set; } = string.Empty;
        public string ColorRALPrinting { get; set; } = string.Empty;
        public List<(Manufactory publisher, City? city, List<City> cityList)> ManufactoryTupleList { get; set; } = [];
        public List<string> ManufactoryIDCityIDList { get; set; } = [];
        public PostcardPotential PostcardPotential { get; set; } = new();
        public PostcardScan PostcardScan { get; set; } = new();
        public List<PostcardScan> PostcardScanList { get; set; } = [];
        public Person PersonSender { get; set; } = new();
        public Person PersonReceiver { get; set; } = new();
        public (Person Person, City? City) PersonReceiverTuple { get; set; } = new();
        public Person AuthorArtist { get; set; } = new();
        public Printing Printing { get; set; } = new();
        public Era Era { get; set; } = new() { EraLong = string.Empty };
        public PostcardEntity PostcardEntity { get; set; } = new();
        //public int ImagePerception { get; set; }
        public string? PriceString { get; set; }
        public PostcardImprint PostcardImprint { get; set; } = new();
        public UsingIdentityUser UsingIdentityUser { get; set; } = new();
        public Manufactory Manufactory { get; set; } = new() { ManufactoryName = string.Empty };
        //public ManufacturingDate ManufacturingDate { get; set; } = new();
    }
}