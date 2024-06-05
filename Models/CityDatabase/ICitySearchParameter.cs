namespace Sammlerplattform.Models.CityDatabase
{
    public interface ICitySearchParameter
    {
        ICollection<int> SearchCity_ID { get; set; }
        ICollection<string> SearchOeconym { get; set; }
        ICollection<string> SearchPostalcode { get; set; }
        ICollection<string> SearchByname { get; set; }
        ICollection<string> SearchGeography { get; set; }
        ICollection<string> SearchParentCity { get; set; }
        ICollection<int> SearchParentCity_ID { get; set; }
    }
}
