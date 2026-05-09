namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantOperationParameterModel
    {
        public Participant Participant { get; set; } = new() { ParticipantName = string.Empty };
        public List<int> ConnectedPlaceIdList { get; set; } = [];
        public List<int> ConnectedEraIdList { get; set; } = [];
    }
}
