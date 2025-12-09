using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase
{
    public class Party
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "PartyID", ResourceType = typeof(SharedResources))]
        public int PartyID { get; set; }
        [Display(Name = "PartyName", ResourceType = typeof(SharedResources))]
        public required string PartyName { get; set; }
        [Display(Name = "Description", ResourceType = typeof(SharedResources))]
        public string? PartyDescription { get; set; }
        [Display(Name = "PartyType", ResourceType = typeof(SharedResources))]
        public int PartyTypeInt { get; set; }
        [NotMapped]
        [Display(Name = "PartyType", ResourceType = typeof(SharedResources))]
        public PartyType PartyTypeEnum
        {
            get => (PartyType)PartyTypeInt;
            set => PartyTypeInt = (int)value;
        }
        [Display(Name = "Individual", ResourceType = typeof(SharedResources))]
        public Individual? Individual { get; set; }
        [Display(Name = "Organization", ResourceType = typeof(SharedResources))]
        public Organization? Organization { get; set; }
        [Display(Name = "CollectionItemNPartyList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];

        [Display(Name = "PlaceList", ResourceType = typeof(SharedResources))]
        public List<Place> PlaceList { get; set; } = [];
    }

    public enum PartyType
    {
        [Display(Name = "Individual", ResourceType = typeof(SharedResources))]
        Individual = 0,
        [Display(Name = "Organization", ResourceType = typeof(SharedResources))]
        Organization = 1
    }
}
