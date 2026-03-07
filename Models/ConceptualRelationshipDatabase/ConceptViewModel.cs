using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_Concept_NameRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public string Name { get; set; } = "";

        [Display(Name = "Abbreviation", ResourceType = typeof(SharedResources))]
        public string? Abbreviation { get; set; } // z. B. "MZ", "KS"

        [Display(Name = "IsRootConcept", ResourceType = typeof(SharedResources))]
        public bool IsRootConcept => RootConceptID == 0 || RootConceptID == null;

        [Display(Name = "RootConceptID", ResourceType = typeof(SharedResources))]
        public int? RootConceptID { get; set; }

        [Display(Name = "RootConcept", ResourceType = typeof(SharedResources))]
        public ConceptViewModel? RootConcept { get; set; }
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
        public List<ConceptViewModel> SubConceptList { get; set; } = [];

        [Display(Name = "Description", ResourceType = typeof(SharedResources))]
        public string? Description { get; set; }

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
}