namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptualRelationshipSearchParameterModel
    {
        public List<int> ConceptID { get; set; } = [];
        public List<string> ConceptName { get; set; } = [];
        public List<int> CollectionAreaID { get; set; } = [];
        public List<int> RelationTypeInt { get; set; } = [];
    }
}