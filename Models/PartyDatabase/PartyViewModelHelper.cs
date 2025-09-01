using Sammlerplattform.Services;

namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartyViewModelHelper
    {
        public static PartyViewModel FromDomainModel(Party party)
        {
            string places = party.PlaceList != null ?
                        string.Join(", ", party.PlaceList.Select(p => p.PlaceNToponymyList.Where(t => t.IsCurrentName).Select(t => t.Toponymy.ToponymyName).FirstOrDefault())) : "Keine Orte zugewiesen";
            string productionFacility = party.Organization?.ProductionFacility != null ?
                        party.Organization.ProductionFacility.ProductionFacilityName : string.Empty;

            return new PartyViewModel
            {
                Name = party.PartyName,
                Description = party.PartyDescription ?? string.Empty,
                Pseudonym = party.Individual?.Pseudonym ?? string.Empty,
                Signature = party.Individual?.Signature ?? string.Empty,
                Places = string.Join(", ", places),
                OrganizationType = party.Organization?.OrganizationTypeEnum.ToString() ?? string.Empty,
    //            OrganizationType = party.Organization != null
    //? party.Organization.OrganizationTypeEnum.GetDisplayName()
    //: string.Empty,
                ProductionFacility = productionFacility
            };
        }
    }
    public class PartyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Pseudonym { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Places { get; set; } = string.Empty;
        public string OrganizationType { get; set; } = string.Empty;
        public string ProductionFacility { get; set; } = string.Empty;
    }
}
