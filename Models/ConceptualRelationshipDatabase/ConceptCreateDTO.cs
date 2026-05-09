using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_Concept_NameRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Abbreviation", ResourceType = typeof(SharedResources))]
        public string? Abbreviation { get; set; } // z. B. "MZ", "KS"

        [Display(Name = "RootConceptID", ResourceType = typeof(SharedResources))]
        public int? RootConceptID { get; set; }

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

        public List<ConceptRelationCreateDTO> ConceptRelationList { get; set; } = [];

        [Display(Name = "IsTranslateable", ResourceType = typeof(SharedResources))]
        public bool IsTranslateable { get; set; } = true;
    }

    public class ConceptRelationCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_Concept_IdRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_Concept_IdRequired", ErrorMessageResourceType = typeof(SharedResources))]
        public int ToConceptId { get; set; }

        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_RelationType_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0, 1, ErrorMessageResourceName = "Error_RelationType_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public int RelationTypeInt { get; set; }
    }
}
