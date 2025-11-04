namespace Sammlerplattform.Models.CollectionItemDatabase.StateDatabase
{
    public class StateSearchParameterModel
    {
        public List<int> StateID { get; set; } = [];
        public List<int> CollectionArea_CollectionAreaID { get; set; } = [];
        public List<string> StateName { get; set; } = [];
        public bool IsGeneralState { get; set; } = true;
    }
}
