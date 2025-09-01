namespace Sammlerplattform.Models.CityDatabase
{
    public class CityOperationParameterModel
    {
        public City City { get; set; } = new();
        public Oeconym Oeconym { get; set; } = new() { OeconymName = string.Empty };
        public List<string> OeconymList { get; set; } = [];
        public List<CityOeconym> CityOeconymList { get; set; } = [];
        public Geography Geography { get; set; } = new() { GeographyName = string.Empty };
        public Postalcode Postalcode { get; set; } = new() { PostalcodeNumber = string.Empty };
        public List<string> PostalcodeNumberList { get; set; } = [];
        public List<CityPostalcode> CityPostalcodeList { get; set; } = [];
        public CityOeconym CityNOeconym { get; set; } = new();
    }
}