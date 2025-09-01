using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ProductionFacility
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductionFacilityID { get; set; }

        [Required(ErrorMessage = "Bitte befüllen.")]
        [StringLength(30)]
        [Display(Name = "Produktionsstätte")]
        [RegularExpression(@"^[a-zA-Z]{1,30}$", ErrorMessage = "Der Name darf nur Buchstaben und max. 30 Zeichen enthalten.")]
        public string ProductionFacilityName { get; set; } = string.Empty;

        public List<Manufactory> ManufactoryList { get; set; } = [];
        public List<Organization> OrganizationList { get; set; } = [];
    }
}