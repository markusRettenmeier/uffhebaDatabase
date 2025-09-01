using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.PartyDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductEntityNParty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductEntityNPersonID { get; set; }
        public int ProductEntityID { get; set; }
        //public BrickEntity BrickEntity { get; set; } = null!;
        public int PartyID { get; set; }
        public Party Party { get; set; } = null!;
        public string? Relationship { get; set; }
    }
}
