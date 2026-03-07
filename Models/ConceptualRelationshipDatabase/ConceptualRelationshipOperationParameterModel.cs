namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptualRelationshipOperationParameterModel
    {
        public ConceptViewModel ConceptViewModel { get; set; } = new() { Name = string.Empty};
        public List<ConceptRelationViewModel> ConceptRelationList { get; set; } = [];
    }
}