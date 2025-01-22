using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    // TPC-Inheritance
    public class ProductEntity<TConditionType>
    {
        [Display(Name = "Ablageort")]
        [StringLength(50)]
        public string? FilingLocation { get; set; }
        [StringLength(3)]
        public string? Charge { get; set; }

        [Display(Name = "Preis")]
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        [Precision(18, 2)]
        public decimal? Price { get; set; }

        [Display(Name = "Fälschung")]
        public bool Fake { get; set; }

        [Display(Name = "Material")]
        [NotMapped]
        public MaterialType MaterialEnum
        {
            get
            {
                return (MaterialType)MaterialInt;
            }
            set
            {
                MaterialInt = (int)value;
            }
        }
        public int MaterialInt { get; set; }
        public string UsingIdentityUsers_ID { get; set; } = string.Empty;

        [Display(Name = "Breite")]
        public int? Width { get; set; }
        [Display(Name = "Höhe")]
        public int? Height { get; set; }
        [Display(Name = "Länge")]
        public int? Length { get; set; }
        public int? ManufacturingDate_ID { get; set; }
        public ManufacturingDate? ManufacturingDate { get; set; }

        [Display(Name = "Bemerkung")]
        public string? Comment { get; set; }
        public int? Owner_ID { get; set; }
        [Display(Name = "Eigentümer")]
        public Person? Owner { get; set; }
        [Display(Name = "Empfangsdatum")]
        public DateTime? TransferFromOwner { get; set; }

        [Display(Name = "Stückzahl")]
        public int? ProductionSize { get; set; }

        [Display(Name = "Erhaltungszustand")]
        [NotMapped]
        public TConditionType? ConditionEnum
        {
            get
            {
                return (TConditionType)Enum.ToObject(typeof(TConditionType), ConditionInt);
            }
            set
            {
                ConditionInt = Convert.ToInt32(value);
            }
        }
        public int ConditionInt { get; set; }
        public ICollection<ProductPicture> ProductPictureICollection { get; set; } = [];
    }

    public enum MaterialType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Holz")]
        Wood = 1,
        [Description("Kupfer")]
        Copper = 2,
        [Description("Papier")]
        Paper = 3,
        [Description("Lehm")]
        Adobe = 4
    }
}