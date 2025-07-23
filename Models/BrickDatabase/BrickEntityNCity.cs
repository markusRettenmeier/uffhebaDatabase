using Sammlerplattform.Models.CityDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickEntityNCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BrickEntityNCityID { get; set; }
        public int BrickEntityID { get; set; }
        public BrickEntity BrickEntity { get; set; } = null!;
        public int CityID { get; set; }
        public City City { get; set; } = null!;
        public string? Relationship { get; set; }
    }
}
