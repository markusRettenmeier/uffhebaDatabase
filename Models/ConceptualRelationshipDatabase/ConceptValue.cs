using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class ConceptValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ConceptValueID { get; set; }

        [NotMapped]
        [Display(Name = "Text", ResourceType = typeof(SharedResources))]
        public string? ValueString { get; set; }

        [Display(Name = "Number", ResourceType = typeof(SharedResources))]
        public int? ValueInt { get; set; }

        [Display(Name = "Date", ResourceType = typeof(SharedResources))]
        public DateTime? ValueDate { get; set; }

        [Display(Name = "Decimal", ResourceType = typeof(SharedResources))]
        public decimal? ValueDecimal { get; set; }

        [Display(Name = "Bool", ResourceType = typeof(SharedResources))]
        public bool? ValueBool { get; set; }

        [NotMapped]
        public string? ValueDisplay
        {
            get
            {
                if (ValueString is not null)
                {
                    return ValueString;
                }
                else if (ValueInt is not null)
                {
                    return ValueInt.ToString();
                }
                else if (ValueDate is not null)
                {
                    return ValueDate?.ToString("d");
                }
                else if (ValueDecimal is not null)
                {
                    return ValueDecimal?.ToString("0.00");
                }
                else if (ValueBool is not null)
                {
                    return ConceptName;
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        public int ConceptID { get; set; }

        [NotMapped]
        public string? ConceptName { get; set; }

        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemEntityID { get; set; }

        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity? CollectionItemEntity { get; set; }
    }
}