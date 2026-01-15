export const columns = [
    ["PersonalIdentificationNumber", "text"],
    ["SerialNumber", "text"],
    ["FilingLocation", "text"],
    ["CollectionItemEntityID", "number"],
    ["UniqueName", "text"],
    
    ["CollectionAreaName", "text"],

    ["PlaceID", "number"],
    ["PlaceNToponymyList_Toponymy_ToponymyName", "text"],
    ["ToponymyTypeInt", "multiselect"],
    ["Settlement_SettlementNPostalcodeList_Postalcode_PostalcodeNumber", "text"],

    ["PartyID", "number"],
    ["PartyName", "text"], 
    ["PartyTypeInt", "multiselect"],
    ["Individual_Pseudonym", "text"],
    ["Individual_Signature", "text"],
    ["Organization_OrganizationTypeInt", "multiselect"],
    ["Organization_ProductionFacility_ProductionFacilityName", "text"],
    ["Organization_PlaceList_PlaceNToponymyList_Toponymy_ToponymyName", "text"],

    ["ConceptID", "number"],
    ["ConceptName", "text"],
    ["RelationTypeInt", "number"],
    ["CollectionAreaID", "number"],

    ["CollectionSetId", "number"],
    ["CollectionSetName", "number"]
];
