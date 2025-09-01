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
        public int RegionID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
