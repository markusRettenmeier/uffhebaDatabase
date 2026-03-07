using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptRelationViewModel
    {
        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
        public int RelationTypeInt { get; set; }

        [NotMapped]
        public RelationType RelationType
        {
            get => (RelationType)RelationTypeInt; 
            set => RelationTypeInt = (int)value;
        }
        public int FromConceptID { get; set; }
        public int ToConceptID { get; set; }
        public ConceptViewModel? ToConcept { get; set; }
        public bool IsDirected { get; set; } = true; // Gibt an, ob die Beziehung gerichtet ist (z. B. "ist ein Teil von" vs. "ähnlich wie")
    }

    public enum RelationType
    {
        [Display(Name = "SynonymFor", ResourceType = typeof(SharedResources))]
        SynonymFor = 0,
        [Display(Name = "SubTermOf", ResourceType = typeof(SharedResources))]
        SubTermOf = 1
    }
}