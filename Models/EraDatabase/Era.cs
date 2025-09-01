using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.EraDatabase
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ÄraId")]
        public int EraID { get; set; }

        [Display(Name = "Äralangbezeichnung")]
        [StringLength(50)]
        public required string EraName { get; set; }

        [Display(Name = "Ärakurzbezeichnung")]
        [StringLength(10)]
        public string? EraShort { get; set; }

        [Display(Name = "Startjahr")]
        public int? StartYear { get; set; }

        [Display(Name = "Endjahr")]
        public int? EndYear { get; set; }
        [Display(Name = "Ära Beschreibung")]
        public string? EraDescription { get; set; }

        public List<BrickEntity> BrickEntityList { get; set; } = [];
        public List<CityOeconym> CityOeconymList { get; set; } = [];
        public List<CityPostalcode> CityPostalcodeList { get; set; } = [];
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
    }
}
