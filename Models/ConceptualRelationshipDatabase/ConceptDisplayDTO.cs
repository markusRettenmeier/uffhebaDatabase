using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptDisplayDTO
    {
        public ConceptViewModel ConceptViewModel { get; set; } = new() { Name = string.Empty };
        public List<ConceptRelationViewModel> ConceptRelationViewList { get; set; } = [];
    }

    public class ConceptViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public string Name { get; set; } = "";

        [Display(Name = "Abbreviation", ResourceType = typeof(SharedResources))]
        public string? Abbreviation { get; set; } // z. B. "MZ", "KS"

        [Display(Name = "IsRootConcept", ResourceType = typeof(SharedResources))]
        public bool IsRootConcept => RootConceptID == 0 || RootConceptID == null;

        [Display(Name = "RootConceptID", ResourceType = typeof(SharedResources))]
        public int? RootConceptID { get; set; }
        public int GetRootConceptId()
        {
            if (RootConceptID == null || RootConceptID == 0)
            {
                return Id;
            }
            else
            {
                return (int)RootConceptID;
            }
        }
        [Display(Name = "SubConceptNameList", ResourceType = typeof(SharedResources))]
        public List<string> SubConceptNameList { get; set; } = [];

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int? CollectionAreaID { get; set; }

        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public int? ConceptTypeInt { get; set; }

        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public ConceptType ConceptType
        {
            get => ConceptTypeInt.HasValue
                ? (ConceptType)ConceptTypeInt.Value
                : ConceptType.Bool;   // oder Default
            set => ConceptTypeInt = (int)value;
        }

    }

    public class ConceptRelationViewModel
    {
        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
        public int RelationTypeInt { get; set; }

        [NotMapped] //Extra, da bei GraphSQL trotzdem ein Mapping kommt
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