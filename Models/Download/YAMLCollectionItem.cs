namespace Sammlerplattform.Models.Download
{
    public class YAMLCollectionItem
    {
        public bool Immaterial { get; set; }
        public string? SerialNumber { get; set; }
        public string? FilingLocation { get; set; }
        public string? Charge { get; set; }
        public decimal? Price { get; set; }
        public bool Fake { get; set; }
        public string? MaterialEnum { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Length { get; set; }
        public int? ExactYear { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsApproximate { get; set; }
        public string? Comment { get; set; }
        public string? TransferFromOwner { get; set; }
        public int? ProductionSize { get; set; }
        public string? Condition { get; set; }
    }
}
