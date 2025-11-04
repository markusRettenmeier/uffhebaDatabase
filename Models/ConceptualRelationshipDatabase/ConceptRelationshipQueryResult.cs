namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptRelationshipQueryResult
    {
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int RelationshipInt { get; set; }
        public bool IsDirected { get; set; }
    }
}
