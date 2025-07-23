namespace Sammlerplattform.Models.Download
{
    public class YAMLReceiver
    {
        public string? Name { get; set; }
        public string? Street { get; set; }
        public int? Streetnumber { get; set; }
        public YAMLCity? City { get; set; } = new();
    }
}
