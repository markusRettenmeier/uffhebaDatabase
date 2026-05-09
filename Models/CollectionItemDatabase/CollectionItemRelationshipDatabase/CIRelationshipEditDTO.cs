using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase
{
    public class CIRelationshipEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_RelationId_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_RelationId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Display(Name = "Name", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_RelationshipName_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public string Name { get; set; } = null!;
    }
}