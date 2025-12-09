using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.ReliefDatabase
{
    public class Relief
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ReliefID", ResourceType = typeof(SharedResources))]
        public int ReliefID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;
    }
}
