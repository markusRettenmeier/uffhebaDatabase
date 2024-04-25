namespace Sammlerplattform.Models.Download
{
    public class Receiver
    {
        public string? Name { get; set; } 
        public string? Street { get; set; }
        public int? Streetnumber { get; set; }
        public City? City { get; set; } = new();
    }
}
