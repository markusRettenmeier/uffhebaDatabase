using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.PersonDatabase;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.Download
{
    public class YAMLPerson
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Pseudonym { get; set; }
        public string? Signature { get; set; }
        public string? Description { get; set; }
        public string? Relation_To_Brick { get; set; }
    }
}
