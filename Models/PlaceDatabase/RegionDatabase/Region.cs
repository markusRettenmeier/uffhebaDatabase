using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.RegionDatabase
{
    public class Region
    {
        //Choronym
        // Region, Land, Kontinent, Insel
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "RegionID", ResourceType = typeof(SharedResources))]
        public int RegionID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;
    }
}
