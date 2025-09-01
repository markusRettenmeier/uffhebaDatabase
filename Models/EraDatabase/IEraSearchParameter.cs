namespace Sammlerplattform.Models.EraDatabase
{
    public interface IEraSearchParameter
    {
        List<int> EraID { get; set; }
        List<string> EraName { get; set; }
        List<string> EraShort { get; set; }
    }
    public class EraSearchParameterModel : IEraSearchParameter
    {
        public List<string> EraName { get; set; } = [];
        public List<int> EraID { get; set; } = [];
        public List<string> EraShort { get; set; } = [];
    }
}
