using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "OrganizationID", ResourceType = typeof(SharedResources))]
        public int OrganizationID { get; set; }

        [Display(Name = "OrganizationType", ResourceType = typeof(SharedResources))]
        public int OrganizationTypeInt { get; set; }
        [NotMapped]
        [Display(Name = "OrganizationType", ResourceType = typeof(SharedResources))]
        public OrganizationType OrganizationTypeEnum
        {
            get => (OrganizationType)OrganizationTypeInt;
            set => OrganizationTypeInt = (int)value;
        }

        [Display(Name = "PartyID", ResourceType = typeof(SharedResources))]
        public int PartyID { get; set; }
        [Display(Name = "PartyID", ResourceType = typeof(SharedResources))]
        public Party Party { get; set; } = null!;

        [Display(Name = "ProductionFacilityID", ResourceType = typeof(SharedResources))]
        public int? ProductionFacilityID { get; set; }
        [Display(Name = "ProductionFacility", ResourceType = typeof(SharedResources))]
        public ProductionFacility? ProductionFacility { get; set; }
    }

    public enum OrganizationType
    {
        [Display(Name = "Company", ResourceType = typeof(SharedResources))]
        Company = 0,
        [Display(Name = "Institution", ResourceType = typeof(SharedResources))]
        Institution = 1,
        [Display(Name = "Other", ResourceType = typeof(SharedResources))]
        Other = 99
    }
}
