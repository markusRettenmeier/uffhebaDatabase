using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class Concept
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ConceptNameRequired", ErrorMessageResourceType = typeof(SharedResources))]
        [NotMapped]
        [Display(Name = "ConceptName", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; } // z. B. "Mönchziegel", "Klappstuhl"

        [NotMapped]
        [Display(Name = "Abbreviation", ResourceType = typeof(SharedResources))]
        public string? Abbreviation { get; set; } // z. B. "MZ", "KS"

        [NotMapped]
        [Display(Name = "IsRootConcept", ResourceType = typeof(SharedResources))]
        public bool IsRootConcept => RootConceptID == 0 || RootConceptID == null;

        [Display(Name = "RootConceptID", ResourceType = typeof(SharedResources))]
        public int? RootConceptID { get; set; }
        [Display(Name = "RootConcept", ResourceType = typeof(SharedResources))]
        public Concept? RootConcept { get; set; }

        //[NotMapped]
        //public int Level
        //{
        //    get
        //    {
        //        int level = 0;
        //        Concept? currentConcept = this;
        //        while (currentConcept?.RootConceptID != null && currentConcept.RootConceptID != 0)
        //        {
        //            level++;
        //            currentConcept = currentConcept.RootConcept;
        //        }
        //        return level;
        //    }
        //}
        //[NotMapped]
        public int GetRootConceptId()
        {
            if(RootConceptID == null || RootConceptID == 0)
            {
                return Id;
            }
            else
            {
                //return RootConcept?.GetRootConceptId() ?? 0;
                return (int)RootConceptID;
            }
        }

        [Display(Name = "SubConceptList", ResourceType = typeof(SharedResources))]
        public List<Concept> SubConceptList { get; set; } = [];

        [NotMapped]
        [Display(Name = "Description", ResourceType = typeof(SharedResources))]
        public string? Description { get; set; }

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int? CollectionAreaID { get; set; }

        [Display(Name = "CollectionArea", ResourceType = typeof(SharedResources))]
        public CollectionArea? CollectionArea { get; set; }

        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public int ConceptTypeInt { get; set; } = 0;

        [NotMapped]
        [Display(Name = "ConceptType", ResourceType = typeof(SharedResources))]
        public ConceptType ConceptType
        {
            get => (ConceptType)ConceptTypeInt;
            set => ConceptTypeInt = (int)value;
        }

        [Display(Name = "IsRequiredConcept", ResourceType = typeof(SharedResources))]
        public bool IsRequired { get; set; }

        [Display(Name = "ConceptValueList", ResourceType = typeof(SharedResources))]
        public List<ConceptValue> ConceptValueList { get; set; } = [];
    }

    public enum ConceptType
    {
        [Display(Name = "Bool", ResourceType = typeof(SharedResources))]
        Text = 0,
        [Display(Name = "Number", ResourceType = typeof(SharedResources))]
        Number = 1,
        [Display(Name = "Date", ResourceType = typeof(SharedResources))]
        Date = 2,
        [Display(Name = "Decimal", ResourceType = typeof(SharedResources))]
        Decimal = 3,
        [Display(Name = "Text", ResourceType = typeof(SharedResources))]
        Bool = 4
    }
}
