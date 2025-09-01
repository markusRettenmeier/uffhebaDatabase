namespace Sammlerplattform.Models.PlaceDatabase.RegionDatabase
{
    public class RegionOperationParameterModel : PlaceOperationParameterModel
    {
        public Region Region { get; set; } = new();
    }
}
