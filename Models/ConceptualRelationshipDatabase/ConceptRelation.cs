using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptRelation
    {
        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
        public int RelationTypeInt { get; set; }
        [NotMapped]
        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
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
        [Display(Name = "SynonymFor", ResourceType = typeof(SharedResources))]
        SynonymFor = 0,
        [Display(Name = "SubTermOf", ResourceType = typeof(SharedResources))]
        SubTermOf = 1,
        [Display(Name = "ShortFor", ResourceType = typeof(SharedResources))]
        ShortFor = 2
    }
}