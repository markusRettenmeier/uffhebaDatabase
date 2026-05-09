using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase
{
    public class CIRelationshipCreateDTO
    {
        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_RelationshipName_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public string Name { get; set; } = null!;
    }
}
