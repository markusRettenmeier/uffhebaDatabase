using Sammlerplattform.Models.CityDatabase;

namespace Sammlerplattform.Models.PersonDatabase
{
    public class PersonOperationParameterModel
    {
        public Person Person { get; set; } = new() { Name = string.Empty };
        public City City { get; set; } = new();
        public List<int> CityIDList { get; set; } = [];
        public Prize Prize { get; set; } = new() { Name = string.Empty };
        public List<string> PrizeList { get; set; } = [];
    }
}
