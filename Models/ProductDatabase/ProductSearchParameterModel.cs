namespace Sammlerplattform.Models.ProductDatabase
{
    public interface IProductSearchParameterModel
    {
        // Potential
        List<string> Serialnumber { get; set; }
        // Entity
        List<string> FilingLocation { get; set; }
        List<decimal> Price { get; set; }
        string? Fake { get; set; }
        List<string> Material { get; set; }
        List<string> UserName { get; set; }
        List<string> Condition { get; set; }
        List<int> Width { get; set; }
        List<int> Height { get; set; }
        List<int> Length { get; set; }
        List<int> ProductionSize { get; set; }
        int StartYear { get; set; }
        int EndYear { get; set; }
    }
}
