using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.StampDatabase
{
    public class StampScan
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StampScan_Id { get; set; }
        public string? Pictures_Format { get; set; }
        public bool Frontside { get; set; }
        public int StampEntity_ID { get; set; }
    }
}
