using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class SettlementNPostalcode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "SettlementNPostalcodeID", ResourceType = typeof(SharedResources))]
        public int SettlementNPostalcodeID { get; set; }

        [Required]
        [Display(Name = "SettlementID", ResourceType = typeof(SharedResources))]
        public int SettlementID { get; set; }
        [Display(Name = "Settlement", ResourceType = typeof(SharedResources))]
        public Settlement Settlement { get; set; } = null!;

        [Required]
        [Display(Name = "PostalcodeID", ResourceType = typeof(SharedResources))]
        public int PostalcodeID { get; set; }
        [Display(Name = "Postalcode", ResourceType = typeof(SharedResources))]
        public Postalcode Postalcode { get; set; } = null!;

        [Display(Name = "IsCurrentPostalcode", ResourceType = typeof(SharedResources))]
        public bool IsCurrentPostalcode { get; set; }
    }
}
