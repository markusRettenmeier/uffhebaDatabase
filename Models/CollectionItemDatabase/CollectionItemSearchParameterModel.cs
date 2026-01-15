namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemSearchParameterModel
    {
        public List<int> CollectionItemEntityID { get; set; } = [];
        public List<int> CollectionAreaID { get; set; } = [];
        public List<string> UniqueName { get; set; } = [];
        public List<string> PersonalIdentificationNumber { get; set; } = [];
        public List<string> FilingLocation { get; set; } = [];
        public List<decimal> DeliveryPrice { get; set; } = [];
        public List<DateTime> DeliveryDate { get; set; } = [];
        public List<string> DeliveryAdress { get; set; } = [];
        public bool Fake { get; set; }
        public List<string> UsingIdentityUsersID { get; set; } = [];
        public List<int> Width { get; set; } = [];
        public List<int> Height { get; set; } = [];
        public List<int> Length { get; set; } = [];
        public List<int> Diameter { get; set; } = [];
        public List<int> Weight { get; set; } = [];
        public List<string> Comment { get; set; } = [];
        public List<DateTime> TransferFromOwner { get; set; } = [];
        public List<int> ProductionSize { get; set; } = [];
        public List<int> StateInt { get; set; } = [];
        public List<int> ExactYear { get; set; } = [];
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public List<string> Material { get; set; } = [];
        public List<string> UsingIdentityUser_UserName { get; set; } = [];
        public List<string> Inscription { get; set; } = [];
        public List<string> Era_EraName { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_PartyName { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_PartyTypeInt { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_Individual_Pseudonym { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_Individual_Signature { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_Organization_OrganizationTypeInt { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_Organization_ProductionFacility_ProductionFacilityName { get; set; } = [];
        public List<string> CollectionItemEntityNPartyList_Party_PlaceList_PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
        public List<string> CollectionItemEntityNPlaceList_Place_PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
        public List<string> CollectionItemEntityNPlaceList_Place_ToponymyTypeInt { get; set; } = [];
        public List<string> CollectionItemEntityNPlaceList_Place_Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber { get; set; } = [];


        // Neue Properties für Embedding-Suche
        public string? SemanticSearchQuery { get; set; }
        public float? MinimumSimilarityScore { get; set; }
        public bool? UseHybridSearch { get; set; }
    }
}
