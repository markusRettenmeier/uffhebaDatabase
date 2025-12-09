using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class Settlement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "SettlementID", ResourceType = typeof(SharedResources))]
        public int SettlementID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;

        [Display(Name = "RelatedGeographyID", ResourceType = typeof(SharedResources))]
        public int? RelatedGeographyID { get; set; }
        [Display(Name = "RelatedGeography", ResourceType = typeof(SharedResources))]
        public Place? RelatedGeography { get; set; }

        [Display(Name = "Byname", ResourceType = typeof(SharedResources))]
        [StringLength(50)]
        public string? Byname { get; set; }
        [Display(Name = "SettlementNPostalcodeList", ResourceType = typeof(SharedResources))]
        public List<SettlementNPostalcode> SettlementNPostalcodeList { get; set; } = [];
    }
}