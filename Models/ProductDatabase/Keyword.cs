using Sammlerplattform.Models.CollectionDatabase
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class Keyword
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int KeywordID { get; set; }
        public required string KeywordName { get; set; }
        //public string HexCode { get; set; } = "#000000"; // Default to black if not specified
        public List<ProductNKeyword> ProductNKeywordList { get; set; } = [];
        //public required string Topic { get; set; }
        public int CollectionID { get; set; }
        public Collection Collection { get; set; } = null!;
    }
}
