namespace Sammlerplattform.Models
{
#pragma warning disable IDE1006 // Benennungsstile
#pragma warning disable CS8981 // Der Typname enthält nur ASCII-Zeichen in Kleinbuchstaben. Solche Namen können möglicherweise für die Sprache reserviert werden.
    public class Rootobject
    {
        public string? batchcomplete { get; set; }
        public Continue _continue { get; set; } = new();
        public Query query { get; set; } = new();
    }

    public class Continue
    {
        public int gsroffset { get; set; }
        public string? _continue { get; set; }
    }

    public class Query
    {
        public Dictionary<string, pageval> pages { get; set; } = [];
    }

    public class pageval
    {
        public int pageid { get; set; }
        public int ns { get; set; }
        public string title { get; set; } = string.Empty;
        public int index { get; set; }
        public Category[]? categories { get; set; }
    }

    public class Category
    {
        public int ns { get; set; }
        public string title { get; set; } = string.Empty;
    }
#pragma warning restore IDE1006 // Benennungsstile
#pragma warning restore CS8981 // Der Typname enthält nur ASCII-Zeichen in Kleinbuchstaben. Solche Namen können möglicherweise für die Sprache reserviert werden.
}
