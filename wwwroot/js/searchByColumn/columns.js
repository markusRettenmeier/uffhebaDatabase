export const columns = [
    ["BrickPotential_BricknameSynonymList_Name", "text"],
    ["BrickPotential_Usage", "multiselect"],
    ["FilingLocation", "text"],
    ["Price", "number"],
    ["Fake", "checkbox"],
    ["Material", "text"],
    ["UsingIdentityUser_UserName", "text"],
    ["Condition", "multiselect"],
    ["ManufacturingYear", "year"],

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

];
export const columnsSimple = columns.filter(([name]) =>
  [

  ].includes(name)
);
