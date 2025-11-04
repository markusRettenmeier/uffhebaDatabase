using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int OrganizationID { get; set; }

        public int OrganizationTypeInt { get; set; }
        [NotMapped]
        public OrganizationType OrganizationTypeEnum
        {
            get => (OrganizationType)OrganizationTypeInt;
            set => OrganizationTypeInt = (int)value;
        }

        public int PartyID { get; set; }
        public Party Party { get; set; } = null!;

        public int? ProductionFacilityID { get; set; }
        public ProductionFacility? ProductionFacility { get; set; }
    }

    public enum OrganizationType
    {
        Company = 0,
        Institution = 1,
        Other = 99
    }
}
