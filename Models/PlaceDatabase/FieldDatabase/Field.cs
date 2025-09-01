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
        public int FieldID { get; set; }

        [Required]
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
