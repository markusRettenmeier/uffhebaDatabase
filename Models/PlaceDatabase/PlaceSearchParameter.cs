namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceSearchParameter
    {
        public List<int> PlaceID { get; set; } = [];
        public List<string> PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
        public List<int> ToponymyTypeInt { get; set; } = [];
        public List<string> Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber { get; set; } = [];
    }
}

