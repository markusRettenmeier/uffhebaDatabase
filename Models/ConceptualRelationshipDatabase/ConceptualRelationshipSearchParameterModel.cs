namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptualRelationshipSearchParameterModel
    {
        public List<int> Id { get; set; } = [];
        public List<int> CollectionAreaID { get; set; } = [];
        public List<int> ConceptTypeInt { get; set; } = [];
        public List<int?> RootConceptID { get; set; } = [];
        public List<string> ConceptName { get; set; } = [];
    }
}