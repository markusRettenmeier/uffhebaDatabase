using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.PlaceDatabase.Toponymy
{
    public class Toponymy
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ToponymyID { get; set; }
        public required string ToponymyName { get; set; }
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
    }
}