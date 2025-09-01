namespace Sammlerplattform.Models.PartyDatabase.OrganizationDatabase
{
    public class OrganizationOperationParameterModel : PartyOperationParameterModel
    {
        public Organization Organization { get; set; } = new();
    }
}
