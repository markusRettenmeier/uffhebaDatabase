using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    public class PostcardEntityNManufacturerNCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PostcardEntityNManufacturerNCity_ID { get; set; }
        public int PostcardEntity_ID { get; set; }
        public int Publisher_ID { get; set; }
        public int? City_ID { get; set; }
    }
}