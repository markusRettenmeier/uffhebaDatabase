namespace Sammlerplattform.Models.ProductDatabase
{
    public interface IProductSearchParameterModel
    {
        // Potential
        public List<string> Serialnumber { get; set; }
        // Entity
        public List<string> FilingLocation { get; set; }
        public List<decimal> Price { get; set; }
        public string? Fake { get; set; }
        public List<string> Material { get; set; }
        public List<string> UserName { get; set; }
        public List<string> Condition { get; set; }
        public List<int> Width { get; set; }
        public List<int> Height { get; set; }
        public List<int> Length { get; set; }
        public List<int> ProductionSize { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}
