namespace Sammlerplattform.Models.PlaceDatabase
{
    public class PlaceViewModelHelper
    {
        public static PlaceViewModel FromDomainModel(Place place) 
        {
            var toponymys = place.PlaceNToponymyList
                .Select(x => x.IsCurrentName
                    ? $"<strong>{x.Toponymy.ToponymyName}</strong>"
                    : x.Toponymy.ToponymyName)
                .ToList();
            var parentPlace = place.ParentPlace?.PlaceNToponymyList
                .FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName ?? string.Empty;
            var postalcodes = place.Settlement?.SettlementNPostalcodeList?
                .Select(x => x.IsCurrentPostalcode
                    ? $"<strong>{x.Postalcode.PostalcodeNumber}</strong>"
                    : x.Postalcode.PostalcodeNumber)
                .ToList();
            var relatedPlace = place.Settlement?.RelatedPlace?.PlaceNToponymyList?
                .FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName ?? string.Empty;
            var childPlaces = place.ChildPlaceList.Select(x => x.PlaceNToponymyList.Where(x => x.IsCurrentName).FirstOrDefault()?.Toponymy.ToponymyName);

            PlaceViewModel placeViewModel = new()
            {
                PlaceID = place.PlaceID,
                ToponymyHtml = string.Join(", ", toponymys ?? []),
                ParentPlaceName = parentPlace,
                Postalcodes = string.Join(", ", postalcodes ?? []),
                RelatedPlaceName = relatedPlace,
                Byname = place.Settlement?.Byname ?? string.Empty,
                ChildPlaces = string.Join(",", childPlaces ?? [])                
            };
            if(!string.IsNullOrEmpty(placeViewModel.Postalcodes))
                placeViewModel.FurtherSpecs = "PLZ: " + placeViewModel.Postalcodes + ", ";
            if(!string.IsNullOrEmpty(placeViewModel.RelatedPlaceName))
                placeViewModel.FurtherSpecs += "Geo: " + placeViewModel.RelatedPlaceName + ", ";
            if (!string.IsNullOrEmpty(placeViewModel.Byname))
                placeViewModel.FurtherSpecs += "Beiname: " + placeViewModel.Byname + ", ";
            if (!string.IsNullOrEmpty(placeViewModel.ParentPlaceName))
                placeViewModel.FurtherSpecs += "Teil von: " + placeViewModel.ParentPlaceName + ", ";
            if(!string.IsNullOrEmpty(placeViewModel.ChildPlaces))
                placeViewModel.FurtherSpecs += "Enthält: " + placeViewModel.ChildPlaces + ", ";

            return placeViewModel;
        }
    }

    public class PlaceViewModel
    {
        public int PlaceID { get; set; }
        public string ToponymyHtml { get; set; } = string.Empty;
        public string ParentPlaceName { get; set; } = string.Empty;
        public string ChildPlaces { get; set; } = string.Empty;
        public string Postalcodes { get; set; } = string.Empty;
        public string RelatedPlaceName { get; set; } = string.Empty;
        public string Byname { get; set; } = string.Empty;
        public string FurtherSpecs { get; set; } = string.Empty;
    }
}
