using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class Color
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ColorID { get; set; }
        public required string Name { get; set; }
        public List<CollectionItemNColor> CollectionItemNColorList { get; set; } = [];
    }
}
