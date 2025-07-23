using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.PostcardDatabase;
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
        public int? PostcardEntity_ID { get; set; }
        public PostcardEntity? PostcardEntity { get; set; }
        public int KeywordID { get; set; }
        public Keyword Keyword { get; set; } = null!;
    }
}
