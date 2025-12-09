using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.FieldDatabase
{
    public class FieldOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "Field", ResourceType = typeof(SharedResources))]
        public Field Field { get; set; } = new();
    }
}
