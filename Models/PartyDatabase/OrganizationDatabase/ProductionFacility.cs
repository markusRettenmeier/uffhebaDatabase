using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class ProductionFacility
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ProductionFacilityID", ResourceType = typeof(SharedResources))]
        public int ProductionFacilityID { get; set; }

        [Required]
        [StringLength(30)]
        [RegularExpression(@"^[a-zA-Z]{1,30}$")]
        [Display(Name = "ProductionFacilityName", ResourceType = typeof(SharedResources))]
        public string ProductionFacilityName { get; set; } = string.Empty;
        [Display(Name = "OrganizationList", ResourceType = typeof(SharedResources))]
        public List<Organization> OrganizationList { get; set; } = [];
    }
}