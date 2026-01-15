using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase
{
    public class Toponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ToponymyID { get; set; }

        [Required(ErrorMessageResourceName = "Error_PlaceName_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(80)]
        [Display(Name = "PlaceName", ResourceType = typeof(SharedResources))]
        [NotMapped]
        public required string ToponymyName { get; set; }

        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
    }
}
