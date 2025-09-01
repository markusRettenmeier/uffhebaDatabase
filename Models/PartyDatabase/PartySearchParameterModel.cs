namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartySearchParameterModel
    {
        public List<int> PartyID { get; set; } = [];
        public List<string> PartyName { get; set; } = [];
        public List<int> PartyTypeInt { get; set; } = [];
        public List<string> Individual_Pseudonym { get; set; } = [];
        public List<string> Individual_Signature { get; set; } = [];
        public List<int> Organization_OrganizationTypeInt { get; set; } = [];
        public List<string> Organization_ProductionFacility_ProductionFacilityName { get; set; } = [];
        public List<string> PlaceList_PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
    }
}
