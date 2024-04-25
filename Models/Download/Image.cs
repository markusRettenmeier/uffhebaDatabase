namespace Sammlerplattform.Models.Download
{
    public class Image
    {
        public double? Height { get; set; } 
        public double? Width { get; set; }
        public int? ColorProcessing { get; set; }
        public string? ImageColor { get; set; }
        public int? ImageYear { get; set; }
            public string? EraLong { get; set; }
        public string? EraShort { get; set; }/*, int? ImagePerception*/
        public bool? Passepartout { get; set; }
        public bool? FullScreen { get; set; }
        public int? CirculationSize { get; set; }
            public string? Buildings { get; set; }
        public int? Technique { get; set; }
        public int? Style { get; set; }
    }
}
