using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.FieldDatabase
{
    public class Field
    {
        //Agronym
        //Flurnamen, Berge, Wälder
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "FieldID", ResourceType = typeof(SharedResources))]
        public int FieldID { get; set; }

        [Required]
        [Display(Name = "PlaceID", ResourceType = typeof(SharedResources))]
        public int PlaceID { get; set; }
        [Display(Name = "Place", ResourceType = typeof(SharedResources))]
        public Place Place { get; set; } = null!;
    }
}
