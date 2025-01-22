namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public class ManufactorySearchParameterModel
    {
        public ICollection<string> SearchManufactory { get; set; } = [];
        public ICollection<string> SearchOeconym { get; set; } = [];
        public ICollection<string> SearchProductionFacility { get; set; } = [];
        public ICollection<int> SearchYear { get; set; } = [];
        public ICollection<string> SearchEraLong { get; set; } = [];
        public ICollection<string> SearchEraShort { get; set; } = [];
    }
}