using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.ProductDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PartyDatabase
{
    public class Party
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PartyID { get; set; }
        public required string PartyName { get; set; }
        public string? PartyDescription { get; set; }
        public int PartyTypeInt { get; set; }
        [NotMapped]
        public PartyType PartyTypeEnum
        {
            get => (PartyType)PartyTypeInt;
            set => PartyTypeInt = (int)value;
        }
        public Individual? Individual { get; set; }
        public Organization? Organization { get; set; }
        public List<ProductEntityNParty> ProductEntityNPartyList { get; set; } = [];

        [Display(Name = "Standorte")]
        public List<Place> PlaceList { get; set; } = [];
    }

    public enum PartyType
    {
        Individual = 0,
        Organization = 1
    }
}
