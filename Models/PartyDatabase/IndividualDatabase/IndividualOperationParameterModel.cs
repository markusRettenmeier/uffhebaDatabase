namespace Sammlerplattform.Models.PartyDatabase.IndividualDatabase
{
    public class IndividualOperationParameterModel : PartyOperationParameterModel
    {
        public Individual Individual { get; set; } = new();
    }
}
