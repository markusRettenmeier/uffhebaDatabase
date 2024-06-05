using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class PostcardEntityNManufactoryNCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PostcardEntityNManufactoryNCity_ID { get; set; }
        public int PostcardEntity_ID { get; set; }
        public int Publisher_ID { get; set; }
        public int? City_ID { get; set; }
    }
}