using Sammlerplattform.Models.BrickDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductNKeyword
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductNKeywordID { get; set; }
        public int BrickEntityID { get; set; }
        public BrickEntity BrickEntity { get; set; } = null!;
        public int KeywordID { get; set; }
        public Keyword Keyword { get; set; } = null!;
    }
}
