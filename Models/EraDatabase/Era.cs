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

        [NotMapped]
        [Required(ErrorMessageResourceName = "Era_Name_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "EraName", ResourceType = typeof(SharedResources))]
        [StringLength(50)]
        public required string EraName { get; set; }

        [Display(Name = "StartYear", ResourceType = typeof(SharedResources))]
        public int? StartYear { get; set; }

        [Display(Name = "EndYear", ResourceType = typeof(SharedResources))]
        public int? EndYear { get; set; }

        [Display(Name = "WikipediaUrl", ResourceType = typeof(SharedResources))]
        public string? WikipediaUrl { get; set; }

        [Display(Name = "CollectionItemEntityList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemEntity> CollectionItemEntityList { get; set; } = [];

    }
}
