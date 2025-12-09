using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase
{
    public class TransportRoute
    {
        // Dromonym: A term used to describe a route or path, often in the context of transportation or travel.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "TransportRouteID", ResourceType = typeof(SharedResources))]
        public int TransportRouteID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;
    }
}
