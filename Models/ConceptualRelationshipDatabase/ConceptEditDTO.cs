using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_ConceptId_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_ConceptId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_Concept_NameRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }

        [Display(Name = "Abbreviation", ResourceType = typeof(SharedResources))]
        public string? Abbreviation { get; set; } // z. B. "MZ", "KS"

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

        public List<ConceptRelationEditDTO> ConceptRelationList { get; set; } = [];
        [Display(Name = "IsTranslateable", ResourceType = typeof(SharedResources))]
        public bool IsTranslateable { get; set; } = true;
    }
    public class ConceptRelationEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_Concept_IdRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_ConceptId_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public int ToConceptId { get; set; }

        [Display(Name = "RelationType", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_RelationType_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0, 1, ErrorMessageResourceName = "Error_RelationType_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public int RelationTypeInt { get; set; }
        public string? ToName { get; set; } // Optionaler ToName der Beziehung, z. B. "ist Teil von", "verwandt mit"
    }
}
