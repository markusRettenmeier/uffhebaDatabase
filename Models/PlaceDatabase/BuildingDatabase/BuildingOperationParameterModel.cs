namespace Sammlerplattform.Models.PlaceDatabase.BuildingDatabase
{
    public class BuildingOperationParameterModel : PlaceOperationParameterModel
    {
        public Building Building { get; set; } = new();
    }
}
