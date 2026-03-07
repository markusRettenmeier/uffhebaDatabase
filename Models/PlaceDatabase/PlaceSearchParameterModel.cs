namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceSearchParameterModel
    {
        public List<int> PlaceID { get; set; } = [];
        public List<string> PlaceNToponymyList_Toponymy_ToponymyName { get; set; } = [];
        public List<string> FurtherSpecs { get; set; } = [];
    }
}