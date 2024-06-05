namespace Sammlerplattform.Models.CityDatabase
{
    public class CitySearchParameterModel
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
