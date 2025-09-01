using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class Settlement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int SettlementID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;

        public int? RelatedPlaceID { get; set; }
        public Place? RelatedPlace { get; set; }

        [Display(Name = "Touristischer oder amtlicher Beiname")]
        [StringLength(50)]
        public string? Byname { get; set; }
        public List<SettlementNPostalcode> SettlementNPostalcodeList { get; set; } = [];
        //public List<Manufactory> ManufactoryList { get; set; } = [];
    }
}