using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionAreaDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionAttributeValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionAttributeValueID", ResourceType = typeof(SharedResources))]
        public int CollectionAttributeValueID { get; set; }
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
                    return ValueBool == true ? "Ja" : "Nein";
                }
                else
                {
                    return null;
                }
            }
        }
        [Display(Name = "CollectionAttributeID", ResourceType = typeof(SharedResources))]
        public int CollectionAttributeID { get; set; }

        [Display(Name = "CollectionAttribute", ResourceType = typeof(SharedResources))]
        public CollectionAttribute CollectionAttribute { get; set; } = null!;

        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemEntityID { get; set; }

        [Display(Name = "CollectionItemEntity", ResourceType = typeof(SharedResources))]
        public CollectionItemEntity? CollectionItemEntity { get; set; }

        [Display(Name = "CollectionItemPotentialID", ResourceType = typeof(SharedResources))]
        public int? CollectionItemPotentialID { get; set; }

        [Display(Name = "CollectionItemPotential", ResourceType = typeof(SharedResources))]
        public CollectionItemPotential? CollectionItemPotential { get; set; }
    }
}