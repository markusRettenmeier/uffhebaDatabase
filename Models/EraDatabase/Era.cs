using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.EraDatabase
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EraID { get; set; }
        public string? WikipediaUrl { get; set; }
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];
        public List<ParticipantNEra> ParticipantNEraList { get; set; } = [];
    }
}
