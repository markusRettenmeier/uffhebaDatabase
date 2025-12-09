namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceSearchParameter
    {
        public List<int> PlaceID { get; set; } = [];
        public List<string> PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
        public List<int> PlaceNToponymyList_Toponymy_ToponymyID { get; set; } = [];
        public List<int> ToponymyTypeInt { get; set; } = [];
        public List<string> Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber { get; set; } = [];
        public List<string> Settlement_Byname { get; set; } = [];
        public List<string> Settlement_RelatedGeography_PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
    }
}

