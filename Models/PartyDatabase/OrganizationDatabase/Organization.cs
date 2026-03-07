using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int OrganizationID { get; set; }
        public int PartyID { get; set; }
        public Party Party { get; set; } = null!;
        public int? IndustryID { get; set; }
        [Display(Name = "Industry", ResourceType = typeof(SharedResources))]
        public Industry? Industry { get; set; }
    }
}
