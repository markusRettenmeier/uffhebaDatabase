namespace Sammlerplattform.Models.ProcessOfManufactureDatabase
{
    public class ProcessOfManufactureSearchParameter
    {
        public List<int> ProcessOfManufactureID { get; set; } = [];
        public List<string> Mainprocess { get; set; } = [];
        public List<string> ProcessOfManufactureName { get; set; } = [];
        public List<string> Technique { get; set; } = [];
        public List<string> Description { get; set; } = [];
    }
}
