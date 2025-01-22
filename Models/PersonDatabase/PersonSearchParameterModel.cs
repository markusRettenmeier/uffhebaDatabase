namespace Sammlerplattform.Models.PersonDatabase
{
    public interface IPersonSearchParameterModel
    {
        public ICollection<int> SearchPersonID { get; set; }
        public ICollection<string> SearchName { get; set; }
        public ICollection<string> SearchSignature { get; set; }
        public ICollection<string> SearchPseudonym { get; set; }
        public ICollection<int> SearchBirthYear { get; set; }
        public ICollection<int> SearchDeathYear { get; set; }
    }

    public class PersonSearchParameterModel : IPersonSearchParameterModel
    {
        public ICollection<int> SearchPersonID { get; set; } = [];
        public ICollection<string> SearchName { get; set; } = [];
        public ICollection<string> SearchSignature { get; set; } = [];
        public ICollection<string> SearchPseudonym { get; set; } = [];
        public ICollection<int> SearchBirthYear { get; set; } = [];
        public ICollection<int> SearchDeathYear { get; set; } = [];
    }
}
