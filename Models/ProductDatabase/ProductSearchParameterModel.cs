namespace Sammlerplattform.Models.ProductDatabase
{
    public interface IProductSearchParameterModel
    {
        // Potential
        ICollection<string> SearchSerialnumber { get; set; }
        // Entity
        ICollection<string> SearchFilingLocation { get; set; }
        ICollection<decimal> SearchPrice { get; set; }
        string? SearchFake { get; set; }
        ICollection<string> SearchMaterial { get; set; }
        ICollection<string> SearchUser { get; set; }
        ICollection<string> SearchCondiiton { get; set; }
        ICollection<int> SearchWidth { get; set; }
        ICollection<int> SearchHeight { get; set; }
        ICollection<int> SearchLength { get; set; }
        ICollection<int> SearchProductionSize { get; set; }

    }
}
