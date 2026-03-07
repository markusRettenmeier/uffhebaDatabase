using Sammlerplattform.Models.PlaceDatabase;

namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartyOperationParameterModel
    {
        public Party Party { get; set; } = new() { PartyName = string.Empty };
        //public List<Place> PlaceList { get; set; } = [];
        public List<int> ConnectedPlaceIDList { get; set; } = [];
    }
}
