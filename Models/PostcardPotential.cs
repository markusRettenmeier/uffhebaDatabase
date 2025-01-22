using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ProductDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class PostcardPotential : ProductPotential
    {
        [Display(Name = "Belegnummer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PostcardPotential_ID { get; set; }

        [Display(Name = "Orte")]
        public List<City> CityList { get; set; } = [];

        [Display(Name = "Format")]
        public int? Formats { get; set; }

        [Display(Name = "Kartenart")]
        public int? CardType { get; set; }

        [Display(Name = "Kartenserie")]
        public int? CardSeries { get; set; }
        public int? PostcardImprint_ID { get; set; }
    }
}