using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductDatabase
{
    public class ProductEntity
    {
        [Display(Name = "Einzigartiger Name, z.B. Löwenmensch")]
        public string? UniqueName { get; set; }
        [Display(Name = "persönl. Kennnummer")]
        public string? PersonalIdentificationNumber { get; set; }

        [Display(Name = "Ablageort")]
        [StringLength(50)]
        public string? FilingLocation { get; set; }
        [StringLength(3)]
        public string? Charge { get; set; }

        [Display(Name = "Bezugspreis")]
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        [Precision(18, 2)]
        public decimal? DeliveryPrice { get; set; }
        [Display(Name = "Bezugsdatum")]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "Bezugsadresse")]
        public string? DeliveryAdress { get; set; }

        [Display(Name = "Fälschung")]
        public bool Fake { get; set; }
        public required string UsingIdentityUsersID { get; set; }
        public UsingIdentityUser UsingIdentityUser { get; set; } = null!;

        [Display(Name = "Breite")]
        public int? Width { get; set; }
        [Display(Name = "Höhe")]
        public int? Height { get; set; }
        [Display(Name = "Länge")]
        public int? Length { get; set; }
        [Display(Name = "Durchmesser")]
        public int? Diameter { get; set; }
        [Display(Name = "Gewicht")]
        public int? Weight { get; set; }

        [Display(Name = "Bemerkung")]
        public string? Comment { get; set; }
        [Display(Name = "Empfangsdatum")]
        public DateTime? TransferFromOwner { get; set; }

        [Display(Name = "Produzierte Stückzahl")]
        public int? ProductionSize { get; set; }
        [Display(Name = "Zustand")]
        public int? ConditionID { get; set; }
        public Condition? Condition { get; set; }
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
        public string Zeit
        {
            get
            {
                if (ExactYear != null)
                {
                    return $"{ExactYear}";
                }
                else
                {
                    return StartYear == null && EndYear == null
                        ? "Unbekannt"
                        : StartYear == null ? $"{EndYear} geschätzt" : EndYear == null ? $"{StartYear} geschätzt" : $"{StartYear} - {EndYear}";
                }
            }
        }
        public int? EraId { get; set; }
        [Display(Name = "Epoche")]
        public Era? Era { get; set; }
        public List<ProductNColorVariant> ProductNColorVariantList { get; set; } = [];
        public int? ProcessOfManufactureID { get; set; }
        public ProcessOfManufacture? ProcessOfManufacture { get; set; }
        [Display (Name = "Inschrift")]
        public string? Inscription { get; set; }
        public List<ProductNMaterial> ProductNMaterialList { get; set; } = [];
        public List<ProductNKeyword> ProductNKeywordList { get; set; } = [];
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
        public List<BrickEntityNPerson> BrickEntityNPersonList { get; set; } = [];
        public List<BrickEntityNCity> BrickEntityNCityList { get; set; } = [];
        public List<ProductEntityNParty> ProductEntityNPartyList { get; set; } = [];
        public List<ProductEntityNPlace> ProductEntityNPlaceList { get; set; } = [];
    }
}