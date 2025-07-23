namespace Sammlerplattform.Models.CityDatabase
{
    public class CityOperationParameterModel
    {
        public City City { get; set; } = new();
        public Oeconym Oeconym { get; set; } = new() { OeconymName = string.Empty };
        public List<string> OeconymList { get; set; } = [];
        public Geography Geography { get; set; } = new() { GeographyName = string.Empty };
        public Postalcode Postalcode { get; set; } = new() { PostalcodeNumber = string.Empty };
        public List<string> PostalcodeNumberList { get; set; } = [];
        public CityNOeconym CityNOeconym { get; set; } = new();
    }
}