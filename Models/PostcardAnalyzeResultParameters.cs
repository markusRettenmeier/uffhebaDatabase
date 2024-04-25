namespace Sammlerplattform.Models
{
    public class PostcardAnalyzeResultParameters
    {
        public string? FrontsideNr { get; set; }
        public string? BacksideNr { get; set; }
        public List<string> CityList { get; set; } = [];
        public List<string> PublisherList { get; set; } = [];
        public List<string> AddressList { get; set; } = [];
        public List<string> PostmarkList { get; set; } = [];
        public List<string> BuildingList { get; set; } = [];
        public List<string> AuthorArtistList { get; set; } = [];
        public List<string> TextList { get; set; } = [];
        public List<string> ForeNameList { get; set; } = [];
        public List<string> SurNameList { get; set; } = [];
        public List<string> NameList { get; set; } = [];
        public List<string> DateList { get; set; } = [];
        public List<string> YearList { get; set; } = [];
        public List<string> NumberList { get; set; } = [];
        public List<string> GeographyList { get; set; } = [];
        public List<string> OccasionList { get; set; } = [];
    }
}