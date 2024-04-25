namespace Sammlerplattform.Models.Download
{
    public class Postcard
    {
        public List<City> CitiesOnPostcard { get; set; } = [];
        public int? ProductionYear { get; set; }
        public bool Immaterial { get; set; }
        public string? SerialNumber { get; set; }
        public string? ISBN { get; set; }
        public int? ProductionSize { get; set; }
        public bool OfficialBusiness { get; set; }
        public bool CorrugatedEdge { get; set; }
        public bool Fieldpost { get; set; }
        public int? Formats { get; set; }
        public int? CardType { get; set; }
        public int? CardSeries { get; set; }
            public bool Leporello { get; set; }/*, bool TearOffPostcard, bool SetUpPostcard*/
        public bool Propaganda { get; set; }
        public bool Ornament { get; set; }
        public string? FilingLocation { get; set; }
            public string? Charge { get; set; }
        public decimal? Price { get; set; }
        public bool Fake { get; set; }
        public int? Material { get; set; }
        public string? ColorRALWritingFrontside { get; set; }
            public string? ColorRALPrintingBackside { get; set; }
        public int? ConditionOfCard { get; set; }
        public DateTime? DateInText { get; set; }
            public string? Text { get; set; }
    }
}
