using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.PostcardDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CityID { get; set; }
        public List<CityNOeconym> CityNOeconymList { get; set; } = [];
        public List<PostcardPotential> PostcardPotentialList { get; set; } = [];
        public List<Manufactory> ManufactoryList { get; set; } = [];

        [Display(Name = "Touristischer oder amtlicher Beiname")]
        [StringLength(50)]
        public string? Byname { get; set; }

        [Display(Name = "Namenszusatz (geografisch)")]
        public int? GeographyID { get; set; }
        public Geography? Geography { get; set; }
        public List<Postalcode> PostalcodeList { get; set; } = [];
        public List<Person> PersonList { get; set; } = [];

        [Display(Name = "Ortsteil von")]
        [ForeignKey("ParentCity")]
        public int? ParentCityID { get; set; }
        public City? ParentCity { get; set; }
        public List<City> ChildCity { get; set; } = [];
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
        public List<BrickEntityNCity> BrickEntityNCityList { get; set; } = [];
    }
}