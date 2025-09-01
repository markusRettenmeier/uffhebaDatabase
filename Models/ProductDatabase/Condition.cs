using Sammlerplattform.Models.BrickDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class Condition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ConditionID { get; set; }
        public required string ConditionName { get; set; }
        public List<BrickEntity> ProductEntityList { get; set; } = [];
    }
}
