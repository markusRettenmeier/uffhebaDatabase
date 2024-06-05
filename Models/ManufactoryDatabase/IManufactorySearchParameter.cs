namespace Sammlerplattform.Models.ManufactoryDatabase
{
    public interface IManufactorySearchParameter
    {
        ICollection<string> SearchManufactory { get; set; }
        ICollection<string> SearchOeconym { get; set; }
        ICollection<string> SearchProductionFacility { get; set; }
        ICollection<int> SearchYear { get; set; }
    }
}
