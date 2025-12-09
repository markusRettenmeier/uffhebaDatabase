using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceNToponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "PlaceNToponymyID", ResourceType = typeof(SharedResources))]
        public int PlaceNToponymyID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;

        [Required]
        [Display(Name = "ToponymyID", ResourceType = typeof(SharedResources))]
        public int ToponymyID { get; set; }
        [Display(Name = "ToponymyID", ResourceType = typeof(SharedResources))]
        public Toponymy Toponymy { get; set; } = null!;

        [Display(Name = "IsCurrentName", ResourceType = typeof(SharedResources))]
        public bool IsCurrentName { get; set; }
    }
}
