using System.Globalization;

namespace Sammlerplattform.Models
{
    public class WordCategorizationModel
    {
        public bool Frontside { get; set; } = true;
        //public int CountDetectedText { get; set; } = 0;
        public int Countobjects { get; set; } = 0;
        public int CountErrors { get; set; } = 0;
        public List<string> Publishers { get; set; } = [];
        public List<string> Cities { get; set; } = [];
        public List<string> AuthorArtists { get; set; } = [];
        public List<(int block, string word)> FoundAAs { get; set; } = [];
        public List<(string newAA, double weight)> NewAAs { get; set; } = [];
        public List<string> PostcardCategories { get; set; } = [];
        public List<string> MissSpelleds { get; set; } = [];
        public List<string> Buildings { get; set; } = [];
        public List<string> AuthorArtistKeywordList { get; set; } = [];
        public List<string> OccasionKeywordList { get; set; } = [];
        public List<(int building, List<int> X, List<int> Y, double Score)> BuildingsWithXYPositionTupleList { get; set; } = [];
        public List<(string word, string line)> RightSpelleds { get; set; } = [];
        public List<(string word, List<int> X, List<int> Y, bool Frontside)> WordsWithXYPositions { get; set; } = [];
        public List<(string word, int height)> WordHeight { get; set; } = [];
        public List<(string word, int distanceToCenterX, int distanceToCenterY)> WordPosComparedToCenterOfImg { get; set; } = [];
        public List<string> Streets { get; set; } = [];
        public string? Text { get; set; }
        public List<(string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside)> WordBlockCategorization { get; set; } = [];
        public List<(string content, string category, double prop, bool Frontside)> Blocks { get; set; } = [];
        public CultureInfo? CultureInfo { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public List<int> MaxWordsInBlock { get; set; } = [];
    }
}