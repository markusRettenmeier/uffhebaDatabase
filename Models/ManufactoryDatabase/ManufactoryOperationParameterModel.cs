using Sammlerplattform.Models.CityDatabase;

namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ManufactoryOperationParameterModel
    {
        public Manufactory Manufactory { get; set; } = new() { ManufactoryName = string.Empty };
        public List<int> CityIDList { get; set; } = [];
        public City City { get; set; } = new();
        public ProductionFacility ProductionFacility { get; set; } = new();
    }
}
