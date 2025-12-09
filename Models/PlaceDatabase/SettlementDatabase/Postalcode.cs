using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class Postalcode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "PostalcodeID", ResourceType = typeof(SharedResources))]
        public int PostalcodeID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "PostalcodeNumber", ResourceType = typeof(SharedResources))]
        public required string PostalcodeNumber { get; set; }
        [Display(Name = "SettlementNPostalcodeList", ResourceType = typeof(SharedResources))]
        public List<SettlementNPostalcode> SettlementNPostalcodeList { get; set; } = [];
    }
}
