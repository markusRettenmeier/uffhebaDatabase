using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase
{
    public class TransportRoute
    {
        // Dromonym: A term used to describe a route or path, often in the context of transportation or travel.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int TransportRouteID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
