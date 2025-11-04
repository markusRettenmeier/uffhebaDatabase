using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ConceptualRelationshipDatabase
{
    public class Concept
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ConceptID { get; set; }
        public required string ConceptName { get; set; }  // z. B. "Mönchziegel", "Klappstuhl"
        public string? Description { get; set; }

        public int CollectionAreaID { get; set; }
        public CollectionArea CollectionArea { get; set; } = null!;
        //public bool IsRoot { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    }
}
