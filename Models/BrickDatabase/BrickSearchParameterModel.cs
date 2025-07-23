namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickSearchParameterModel
    {
        // Potential
        public List<string> BrickPotential_BricknameSynonymList_Name { get; set; } = [];
        public List<string> BrickPotential_Usage { get; set; } = [];
        public List<int> BrickEntityID { get; set; } = [];

        // Inherited
        // Potential
        public List<string> BrickPotential_Serialnumber { get; set; } = [];
        // Entity
        public List<string> FilingLocation { get; set; } = [];
        public List<decimal> Price { get; set; } = [];
        public string? Fake { get; set; }
        public List<string> Material { get; set; } = [];
        public List<string> UsingIdentityUsersID { get; set; } = [];
        public List<string> UsingIdentityUser_UserName { get; set; } = [];
        public List<string> Condition { get; set; } = [];
        public List<int> ManufacturingYear { get; set; } = [];
        public List<string> Oeconym { get; set; } = [];
        public List<string> Postalcode { get; set; } = [];
        public List<string> Geography { get; set; } = [];
        public List<string> ManufactoryName { get; set; } = [];
        public List<string> ProductionFacility_ProductionFacilityName { get; set; } = [];
        public List<string> Name { get; set; } = [];
        public List<string> Signature { get; set; } = [];
        public List<string> Pseudonym { get; set; } = [];
        public List<int> BirthYear { get; set; } = [];
        public List<int> DeathYear { get; set; } = [];
        public List<string> Era { get; set; } = [];
    }
}
