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
  
];
export const columnsSimple = columns.filter(([name]) =>
  [

  ].includes(name)
);
