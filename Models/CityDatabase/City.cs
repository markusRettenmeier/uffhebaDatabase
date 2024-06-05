using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CityDatabase
{
    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int City_ID { get; set; }
        public ICollection<CityNOeconym> CityNOeconymICollection { get; set; } = [];
        public List<PostcardPotential> PostcardPotentialList { get; set; } = [];
        public List<Manufactory> ManufactoryList { get; set; } = [];

        [Display(Name = "Touristischer oder amtlicher Beiname")]
        [StringLength(50)]
        public string? Byname { get; set; }

        [Display(Name = "Namenszusatz (geografisch)")]
        public int? Geography_ID { get; set; }
        public Geography? Geography { get; set; }
        public ICollection<Postalcode> PostalcodeICollection { get; set; } = [];
        public Person? Person { get; set; }

        [Display(Name = "Ortsteil von")]
        [ForeignKey("ParentCity")]
        public int? ParentCity_ID { get; set; }
        public City? ParentCity { get; set; }
        public ICollection<City> ChildCity { get; set; } = [];
    }
}