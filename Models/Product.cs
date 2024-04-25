using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models
{
    //TPC-Inheritance
    public abstract class Product
    {
        [Display(Name = "Produkt ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Product_ID { get; set; }

        [Display(Name = "Herstellungsjahr")]
        public int? ProductionYear { get; set; }
        public bool Immaterial { get; set; }

        [Display(Name = "Seriennummer")]
        [StringLength(50)]
        public string? SerialNumber { get; set; }

        [StringLength(13)]
        [Display(Name = "ISBN-Nummer")]
        public string? ISBN { get; set; }

        [Display(Name = "Stückzahl")]
        public int? ProductionSize { get; set; }
    }
}