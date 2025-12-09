using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAttributeOperationParameterModel
    {
        [Display(Name = "CollectionArea", ResourceType = typeof(SharedResources))]
        public CollectionArea CollectionArea { get; set; } = new() { CollectionAreaName = string.Empty };
        [Display(Name = "CollectionAttribute", ResourceType = typeof(SharedResources))]
        public CollectionAttribute CollectionAttribute { get; set; } = new() { CollectionAttributeName = string.Empty };
    }
}
