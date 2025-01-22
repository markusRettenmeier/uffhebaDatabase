namespace Sammlerplattform.Models.CityDatabase
{
    public interface ICitySearchParameterModel
    {
        ICollection<int> SearchCity_ID { get; set; }
        ICollection<string> SearchOeconym { get; set; }
        ICollection<string> SearchPostalcode { get; set; }
        ICollection<string> SearchByname { get; set; }
        ICollection<string> SearchGeography { get; set; }
        ICollection<string> SearchParentCity { get; set; }
        ICollection<int> SearchParentCity_ID { get; set; }
    }
    public class CitySearchParameterModel: ICitySearchParameterModel
    {
        public ICollection<int> SearchCity_ID { get; set; } = [];
        public ICollection<string> SearchOeconym { get; set; } = [];
        public ICollection<string> SearchPostalcode { get; set; } = [];
        public ICollection<string> SearchByname { get; set; } = [];
        public ICollection<string> SearchGeography { get; set; } = [];
        public ICollection<string> SearchParentCity { get; set; } = [];
        public ICollection<int> SearchParentCity_ID { get; set; } = [];
    }
}
