using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class SettlementOperationParameterModel : PlaceOperationParameterModel
    {
        [Display(Name = "Settlement", ResourceType = typeof(SharedResources))]
        public Settlement Settlement { get; set; } = new();
        [Display(Name = "SettlementNPostalcodeList", ResourceType = typeof(SharedResources))]
        public List<SettlementNPostalcode> SettlementNPostalcodeList { get; set; } = [];
    }
}
