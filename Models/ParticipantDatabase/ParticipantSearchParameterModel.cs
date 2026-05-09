namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantSearchParameterModel
    {
        public List<int> ParticipantID { get; set; } = [];
        public List<string> ParticipantName { get; set; } = [];
        public List<int> ParticipantTypeInt { get; set; } = [];
        public List<string> Individual_Pseudonym { get; set; } = [];
        public List<string> Individual_Signature { get; set; } = [];
        public List<int> Organization_Industry_Id { get; set; } = [];
    }
}
