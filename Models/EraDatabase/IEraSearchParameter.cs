namespace Sammlerplattform.Models.EraDatabase
{
    public interface IEraSearchParameter
    {
        ICollection<string> SearchEraLong { get; set; }
        ICollection<string> SearchEraShort { get; set; }
    }
}
