using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class Color
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "ColorID", ResourceType = typeof(SharedResources))]
        public int ColorID { get; set; }
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        public required string Name { get; set; }
        [Display(Name = "CollectionItemNColorList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNColor> CollectionItemNColorList { get; set; } = [];
    }
}