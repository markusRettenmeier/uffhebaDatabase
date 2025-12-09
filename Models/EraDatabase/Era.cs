using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.EraDatabase
{
    public class Era
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "EraID", ResourceType = typeof(SharedResources))]
        public int EraID { get; set; }

        [Display(Name = "EraName", ResourceType = typeof(SharedResources))]
        [StringLength(50)]
        public required string EraName { get; set; }

        [Display(Name = "EraShort", ResourceType = typeof(SharedResources))]
        [StringLength(10)]
        public string? EraShort { get; set; }

        [Display(Name = "StartYear", ResourceType = typeof(SharedResources))]
        public int? StartYear { get; set; }

        [Display(Name = "EndYear", ResourceType = typeof(SharedResources))]
        public int? EndYear { get; set; }
        [Display(Name = "Description", ResourceType = typeof(SharedResources))]
        public string? EraDescription { get; set; }
        [Display(Name = "PlaceNToponymyList", ResourceType = typeof(SharedResources))]
        public List<PlaceNToponymy> PlaceNToponymyList { get; set; } = [];
        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

    }
}
