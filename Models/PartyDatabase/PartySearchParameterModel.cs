namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartySearchParameterModel
    {
        public List<int> PartyID { get; set; } = [];
        public List<string> PartyName { get; set; } = [];
        public List<int> PartyTypeInt { get; set; } = [];
        public List<string> Individual_Pseudonym { get; set; } = [];
        public List<string> Individual_Signature { get; set; } = [];
        public List<string> Organization_Industry_IndustryName { get; set; } = [];
    }
}
