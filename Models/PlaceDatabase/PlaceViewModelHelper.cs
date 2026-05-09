namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceViewModelHelper
    {
        public static PlaceViewModel FromDomainModel(Place place)
        {
            List<string> toponymys = [.. place.PlaceNToponymyList
                .Select(x => x.IsCurrentName
                    ? $"<strong>{x.Toponymy.ToponymyName}</strong>"
                    : x.Toponymy.ToponymyName)];
            List<string> connectedPlaces = [.. place.ConnectedPlaces
                .Select(x => x.PlaceNToponymyList.First(x => x.IsCurrentName).Toponymy.ToponymyName)];

            PlaceViewModel placeViewModel = new()
            {
                PlaceID = place.PlaceID,
                Toponymy = string.Join(", ", toponymys ?? []),
                ConnectedPlaces = string.Join(", ", connectedPlaces ?? []),
                FurtherSpecs = place.FurtherSpecs ?? string.Empty,
                IsDeletable = place.ConnectedPlaces.ToList().Count == 0 && place.ParticipantNPlaceList.Count == 0 && place.CollectionItemNPlaceList.Count == 0
            };

            return placeViewModel;
        }
    }

    public class PlaceViewModel
    {
        public int PlaceID { get; set; }
        public string Toponymy { get; set; } = string.Empty;
        public string ConnectedPlaces { get; set; } = string.Empty;
        public string FurtherSpecs { get; set; } = string.Empty;
        public bool IsDeletable { get; set; }
    }
}
