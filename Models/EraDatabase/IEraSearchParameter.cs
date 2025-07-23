namespace Sammlerplattform.Models.EraDatabase
{
    public interface IEraSearchParameter
    {
        List<string> EraName { get; set; }
    }
    public class EraSearchParameterModel : IEraSearchParameter
    {
        public List<string> EraName { get; set; } = [];
    }
}
