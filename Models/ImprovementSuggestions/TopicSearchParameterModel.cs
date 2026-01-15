namespace Sammlerplattform.Models.ImprovementSuggestions
{
    public class TopicSearchParameterModel
    {
        public List<int> Id { get; set; } = [];
        public List<string> Title { get; set; } = [];
        public List<string> Content { get; set; } = [];
        public List<int> AuthorId { get; set; } = [];
    }
}
