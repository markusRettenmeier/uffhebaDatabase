namespace Sammlerplattform.Models.Download
{
    public class City
    {
        public List<string>? Oeconym { get; set; }
        public List<string> Postalcode { get; set; } = [];
        public string? Byname { get; set; }
        public Geography? Geography { get; set; }
    }
}
