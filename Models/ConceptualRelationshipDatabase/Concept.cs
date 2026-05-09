using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    /// <summary>
    /// Wird benötigt, um die ConceptRelation-Tabelle zu befüllen. ConceptViewModel ist nicht geeignet, da es nicht die nötigen Felder enthält.
    /// Zudem hat ConceptViewModel [NotMapped Felder, die zu Problemen führen würden.
    /// </summary>
    public class Concept : IGraphNode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        public int Id { get; set; }

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
        public int ConceptTypeInt { get; set; } = 0;
    }

    public enum ConceptType
    {
        [Display(Name = "Bool", ResourceType = typeof(SharedResources))]
        Bool = 0,
        [Display(Name = "Number", ResourceType = typeof(SharedResources))]
        Number = 1,
        [Display(Name = "Date", ResourceType = typeof(SharedResources))]
        Date = 2,
        [Display(Name = "Decimal", ResourceType = typeof(SharedResources))]
        Decimal = 3,
        [Display(Name = "Text", ResourceType = typeof(SharedResources))]
        Text = 4
    }
}
