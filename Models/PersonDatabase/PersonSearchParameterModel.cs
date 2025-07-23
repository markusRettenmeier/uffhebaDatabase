namespace Sammlerplattform.Models.PersonDatabase
{
    public interface IPersonSearchParameterModel
    {
        List<int> PersonID { get; set; }
        List<string> Name { get; set; }
        List<string> Signature { get; set; }
        List<string> Pseudonym { get; set; }
        List<int> BirthYear { get; set; }
        List<int> DeathYear { get; set; }
    }

    public class PersonSearchParameterModel : IPersonSearchParameterModel
    {
        public List<int> PersonID { get; set; } = [];
        public List<string> Name { get; set; } = [];
        public List<string> Signature { get; set; } = [];
        public List<string> Pseudonym { get; set; } = [];
        public List<int> BirthYear { get; set; } = [];
        public List<int> DeathYear { get; set; } = [];
    }
}
