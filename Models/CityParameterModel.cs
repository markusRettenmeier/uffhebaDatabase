namespace Sammlerplattform.Models
{
    public class CityParameterModel
    {
        public City City { get; set; } = new();
        public Oeconym Oeconym { get; set; } = new() { OeconymName = string.Empty };
        public List<string> OeconymList { get; set; } = [];
        public Geography Geography { get; set; } = new();
        public Postalcode Postalcode { get; set; } = new() { PostalcodeNumber = string.Empty };
        public List<string> PostalcodeNumberList { get; set; } = [];
        public CityNOeconym CityNOeconym { get; set; } = new();
    }
}