using Sammlerplattform.Models.CollectionAreaDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemValueID { get; set; }
        public string? ValueString { get; set; }
        public int? ValueInt { get; set; }
        public DateTime? ValueDate { get; set; }
        public decimal? ValueDecimal { get; set; }
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
        public int CollectionAttributeID { get; set; }
        public CollectionAttribute CollectionAttribute { get; set; } = null!;
        public int? CollectionItemEntityID { get; set; }
        public CollectionItemEntity? CollectionItemEntity { get; set; }
        public int? CollectionItemPotentialID { get; set; }
        public CollectionItemPotential? CollectionItemPotential { get; set; }
    }
}
