using Sammlerplattform.Models.CollectionItemDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProcessOfManufactureDatabase
{
    public class ProcessOfManufacture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProcessOfManufactureID { get; set; }
        public required string Mainprocess { get; set; } // z.B. Druck, Blasverfahren
        public required string ProcessOfManufactureName { get; set; } // Tampondruck
        public string? Technique { get; set; } // Tiefdruck
        public string? Description { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
    }
}
