using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionDatabase
{
    public class CollectionItemValues
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductEntityNPersonID { get; set; }

        public string? ValueString { get; set; }
        public int? ValueInt { get; set; }
        public DateTime? ValueDate { get; set; }
        public decimal? ValueDecimal { get; set; }
        public int CollectionFieldId { get; set; }
        public CollectionField CollectionField { get; set; } = null!;
        public int? ProductEntityId { get; set; }
        //public BrickEntity? ProductEntity { get; set; }
        public int? ProductPotentialId { get; set; }
    }
}
