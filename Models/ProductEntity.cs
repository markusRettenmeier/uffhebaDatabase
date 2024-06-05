using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.ManufactoryDatabase;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Sammlerplattform.Models
{
    // TPC-Inheritance
    public class ProductEntity<TConditionType>
    {
        [Display(Name = "Ablageort")]
        public string? FilingLocation { get; set; }
        public string? Charge { get; set; }

        [Display(Name = "Preis")]
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        [Precision(18, 2)]
        public decimal? Price { get; set; }

        [Display(Name = "Fälschung")]
        public bool Fake { get; set; }

        [Display(Name = "Material")]
        [NotMapped]
        public MaterialType MaterialEnum { get; set; }
        public int MaterialInt { get; set; }
        public string? UsingIdentityUsers_ID { get; set; }

        [Display(Name = "Erhaltungszustand")]
        [NotMapped]
        public TConditionType? ConditionEnum { get; set; }
        public int ConditionInt { get; set; }

        [Display(Name = "Breite")]
        public int? Width { get; set; }
        [Display(Name = "Höhe")]
        public int? Height { get; set; }
        [Display(Name = "Länge")]
        public int? Length { get; set; }
        //public int? ManufacturingDate_ID { get; set; }
        //public ManufacturingDate? ManufacturingDate { get; set; }

        [Display(Name = "Bemerkung")]
        public string? Comment { get; set; }
        //[Display(Name = "Eigentümer")]
        //public int? Owner_ID { get; set; }
        //public Person? Owner { get; set; }
        //public DateTime TransferFromOwner { get; set; }

        [Display(Name = "Stückzahl")]
        public int? ProductionSize { get; set; }
    }

    public enum MaterialType
    {
        KeineAngabe = 0,
        Holz = 1,
        Kupfer = 2,
        Papier = 3
    }
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}