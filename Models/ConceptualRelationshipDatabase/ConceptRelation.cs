using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptRelation
    {
        //public int ConceptRelationID { get; set; }

        public int RelationTypeInt { get; set; }
        [NotMapped]
        public RelationType RelationType
        {
            get => (RelationType)RelationTypeInt; set => RelationTypeInt = (int)value;
        }

        [NotMapped]
        public int FromConceptID { get; set; }
        //public Concept FromConcept { get; set; } = null!;

        [NotMapped]
        public int ToConceptID { get; set; }
        public Concept ToConcept { get; set; } = null!;

        public bool IsDirected { get; set; } = true; // Gibt an, ob die Beziehung gerichtet ist (z. B. "ist ein Teil von" vs. "ähnlich wie")
    }

    public enum RelationType
    {
        SynonymFor = 0,
        SubTermOf = 1,
        ShortFor = 2
    }
}
