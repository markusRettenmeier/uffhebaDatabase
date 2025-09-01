using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CityID { get; set; }
        public List<CityOeconym> CityOeconymList { get; set; } = [];
        public List<CityPostalcode> CityPostalcodeList { get; set; } = [];
        public List<Manufactory> ManufactoryList { get; set; } = [];

        [Display(Name = "Touristischer oder amtlicher Beiname")]
        [StringLength(50)]
        public string? Byname { get; set; }

        [Display(Name = "Namenszusatz (geografisch)")]
        public int? GeographyID { get; set; }
        public Geography? Geography { get; set; }

        [Display(Name = "Ortsteil von")]
        public int? ParentCityID { get; set; }
        public City? ParentCity { get; set; }
        public List<City> ChildCityList { get; set; } = [];
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
        public List<BrickEntityNCity> BrickEntityNCityList { get; set; } = [];
    }
}