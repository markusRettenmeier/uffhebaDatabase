using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    // TPC-Inheritance
    public class ProductEntity<TConditionType>
    {
        public string? Name { get; set; }

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

        //[Display(Name = "Material")]
        //[NotMapped]
        //public MaterialType MaterialEnum
        //{
        //    get => (MaterialType)MaterialInt; set => MaterialInt = (int)value;
        //}
        //public int MaterialInt { get; set; }
        public required string UsingIdentityUsersID { get; set; }
        public UsingIdentityUser? UsingIdentityUser { get; set; }

        [Display(Name = "Breite")]
        public int? Width { get; set; }
        [Display(Name = "Höhe")]
        public int? Height { get; set; }
        [Display(Name = "Länge")]
        public int? Length { get; set; }
        public int? Radius { get; set; }
        //public int? ManufacturingDate_ID { get; set; }
        //public ManufacturingDate? ManufacturingDate { get; set; }

        [Display(Name = "Bemerkung")]
        public string? Comment { get; set; }
        [Display(Name = "Empfangsdatum")]
        public DateTime? TransferFromOwner { get; set; }

        [Display(Name = "Produzierte Stückzahl")]
        public int? ProductionSize { get; set; }

        [Display(Name = "Erhaltungszustand")]
        [NotMapped]
        public TConditionType? ConditionEnum
        {
            get => (TConditionType)Enum.ToObject(typeof(TConditionType), ConditionInt); set => ConditionInt = Convert.ToInt32(value);
        }
        public int ConditionInt { get; set; }
        public List<ProductPicture> ProductPictureList { get; set; } = [];
        [Display(Name = "Exaktes Jahr")]
        public int? ExactYear { get; set; }
        [Display(Name = "Startjahr")]
        public int? StartYear { get; set; }
        [Display(Name = "Endjahr")]
        public int? EndYear { get; set; }
        [Display(Name = "Ist es geschätzt?")]
        public bool IsApproximate { get; set; }
        [NotMapped]
        public string Zeit {
            get
            {
                if (ExactYear != null)
                    return $"{ExactYear}";
                else
                {
                    if (StartYear == null && EndYear == null)
                        return "Unbekannt";
                    if (StartYear == null)
                        return $"{EndYear} geschätzt";
                    if (EndYear == null)
                        return $"{StartYear} geschätzt";
                    return $"{StartYear} - {EndYear}";
                }
            }
        }
        public int? EraId { get; set; }
        [Display(Name = "Epoche")]
        public Era? Era { get; set; }
        public List<ProductNColorVariant> ProductNColorVariantList { get; set; } = [];
        public int? ProcessOfManufactureID { get; set; }
        public ProcessOfManufacture? ProcessOfManufacture { get; set; }
        public string? Inscription { get; set; }
        public List<ProductNMaterial> ProductNMaterialList { get; set; } = [];
        public List<ProductNKeyword> ProductNKeywordList { get; set; } = [];
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