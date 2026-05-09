using Sammlerplattform.Models.PlaceDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantNPlace
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ParticipantNPlaceId { get; set; }
        public int ParticpantID { get; set; }
        public Participant Participant { get; set; } = null!;
        public int PlaceID { get; set; }
        public Place Place { get; set; } = null!;
    }
}
