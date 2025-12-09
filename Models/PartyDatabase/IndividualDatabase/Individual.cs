using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase.IndividualDatabase
{
    public class Individual
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "IndividualID", ResourceType = typeof(SharedResources))]
        public int IndividualID { get; set; }
        [Display(Name = "Pseudonym", ResourceType = typeof(SharedResources))]
        public string? Pseudonym { get; set; }
        [Display(Name = "Signature", ResourceType = typeof(SharedResources))]
        public string? Signature { get; set; }
        [Display(Name = "PartyID", ResourceType = typeof(SharedResources))]
        public int PartyID { get; set; }
        [Display(Name = "Party", ResourceType = typeof(SharedResources))]
        public Party Party { get; set; } = null!;
    }
}
