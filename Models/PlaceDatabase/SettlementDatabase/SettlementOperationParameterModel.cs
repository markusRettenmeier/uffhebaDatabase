using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.PlaceDatabase.SettlementDatabase
{
    public class SettlementOperationParameterModel : PlaceOperationParameterModel
    {
        public Settlement Settlement { get; set; } = new();
        [Display(Name = "PLZ")]
        public List<SettlementNPostalcode> SettlementNPostalcodeList { get; set; } = [];
    }
}
