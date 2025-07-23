using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class Keyword
    {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Key]
            public int KeywordID { get; set; }
            public required string Name { get; set; }
            //public string HexCode { get; set; } = "#000000"; // Default to black if not specified
            public List<ProductNKeyword> ProductNKeywordList { get; set; } = [];
    }
}
