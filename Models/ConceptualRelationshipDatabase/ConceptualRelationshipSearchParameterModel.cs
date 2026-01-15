namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptualRelationshipSearchParameterModel
    {
        public List<int> Id { get; set; } = [];
        public List<string> Name { get; set; } = [];
        public List<int> CollectionAreaID { get; set; } = [];
        public List<int> ConceptTypeInt { get; set; } = [];        
    }
}