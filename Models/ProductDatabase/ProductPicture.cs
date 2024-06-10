using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductPicture_Id { get; set; }
        public string? FileExtension { get; set; }
        public bool Frontside { get; set; }
        public int PostcardEntity_ID { get; set; }
    }
}