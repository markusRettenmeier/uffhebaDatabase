namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public interface IManufactorySearchParameter
    {
        List<int> ManufactoryID { get; set; }
        List<string> ManufactoryName { get; set; }
        List<int> CityID { get; set; }
        List<string> Oeconym { get; set; }
        List<string> ProductionFacility_ProductionFacilityName { get; set; }
    }
    public class ManufactorySearchParameterModel: IManufactorySearchParameter
    {
        public List<int> ManufactoryID { get; set; } = [];
        public List<string> ManufactoryName { get; set; } = [];
        public List<int> CityID { get; set; } = [];
        public List<string> Oeconym { get; set; } = [];
        public List<string> ProductionFacility_ProductionFacilityName { get; set; } = [];
    }
}
