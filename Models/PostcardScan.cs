using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class PostcardScan
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PostcardScan_Id { get; set; }
        public string? Pictures_Format { get; set; }
        public bool Frontside { get; set; }
        public int PostcardEntity_ID { get; set; }
    }
}