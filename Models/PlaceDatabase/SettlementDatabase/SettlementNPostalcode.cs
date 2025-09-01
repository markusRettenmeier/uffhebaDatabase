using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class SettlementNPostalcode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int SettlementNPostalcodeID { get; set; }

        [Required]
        public int SettlementID { get; set; }
        public Settlement Settlement { get; set; } = null!;

        [Required]
        public int PostalcodeID { get; set; }
        public Postalcode Postalcode { get; set; } = null!;

        //public int? EraID { get; set; }
        //public Era? Era { get; set; }
        public bool IsCurrentPostalcode { get; set; }
    }
}
