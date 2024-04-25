using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    //[Keyless]
    public class PostcardModel
    {
        [Display(Name = "Gibt es ein Bild?")]
        public bool HasImage { get; set; } = false;
        [Display(Name = "Gibt es einen Sender?")]
        public bool HasSender { get; set; } = false;
        [Display(Name = "Gibt es einen Empfänger?")]
        public bool HasReceiver { get; set; } = false;
        public City City { get; set; } = new();
        public List<(City City, List<Postalcode> PostalcodeList, Geography Geography)> CityTupleList { get; set; } = [];
        public List<int> CityIDList { get; set; } = [];
        public Geography Geography { get; set; } = new() { GeographyName = string.Empty };
        public Postalcode Postalcode { get; set; } = new() { PostalcodeNumber = string.Empty };
        public string ColorRALWriting { get; set; } = string.Empty;
        public string ColorImage { get; set; } = string.Empty;
        public string ColorRALPrinting { get; set; } = string.Empty;
        public List<(Manufacturer publisher, City? city, List<City> cityList)> ManufacturerTupleList { get; set; } = [];
        public List<string> ManufacturerIDCityIDList { get; set; } = [];
        public PostcardPotential PostcardPotential { get; set; } = new();
        public PostcardScan PostcardScan { get; set; } = new();
        public List<PostcardScan> PostcardScanList { get; set; } = [];
        public Person PersonSender { get; set; } = new();
        public Person PersonReceiver { get; set; } = new();
        public (Person Person, City? City) PersonReceiverTuple { get; set; } = new();
        public AuthorArtist AuthorArtist { get; set; } = new();
        public Printing Printing { get; set; } = new();
        public Era Eras { get; set; } = new() { EraLong = string.Empty };
        public PostcardEntity PostcardEntity { get; set; } = new();
        //public int ImagePerception { get; set; }
        public string? PriceString { get; set; }
        public PostcardImprint PostcardImprint { get; set; } = new();
        public UsingIdentityUser UsingIdentityUser { get; set; } = new();
        public Manufacturer Manufacturer { get; set; } = new();
        public Oeconym Oeconym { get; set; } = new() { OeconymName = ""};
    }
}