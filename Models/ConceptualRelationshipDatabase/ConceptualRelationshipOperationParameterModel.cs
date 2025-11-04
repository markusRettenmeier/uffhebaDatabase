namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptualRelationshipOperationParameterModel
    {
        public Concept Concept { get; set; } = new() { ConceptName = string.Empty };
        public List<ConceptRelation> ConceptRelationList { get; set; } = [];
    }
}