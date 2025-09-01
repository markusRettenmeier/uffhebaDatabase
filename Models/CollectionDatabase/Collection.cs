using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionDatabase
{
    public class Collection
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionID { get; set; }
        public string CollectionName { get; set; }
    }
}
