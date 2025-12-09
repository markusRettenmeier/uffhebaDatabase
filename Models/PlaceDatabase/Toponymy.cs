using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class Toponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ToponymyID", ResourceType = typeof(SharedResources))]
        public int ToponymyID { get; set; }

        [Required]
        [StringLength(80)]
        [Display(Name = "ToponymyName", ResourceType = typeof(SharedResources))]
        public required string ToponymyName { get; set; }

        [Display(Name = "PlaceNToponymyList", ResourceType = typeof(SharedResources))]
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
    }
}
