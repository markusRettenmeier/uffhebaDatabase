using Sammlerplattform.Data;
using Sammlerplattform.Models.PersonDatabase;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessPerson
    {
        (Person Person, int Statuscode, string StatusMessage) Create(PersonOperationParameterModel personOperationParameterModel);
        (Person Person, int StatusCode, string StatusMessage) Edit(PersonOperationParameterModel personOperationParameterModel);
        List<PersonOperationParameterModel> GetWithPredicates(PersonSearchParameterModel personSearchParameterModel);
    }

    public class PersonProcessor(IUnitOfWork unitOfWork) : IProcessPerson
    {
        public (Person Person, int Statuscode, string StatusMessage) Create(PersonOperationParameterModel personOperationParameterModel)
        {
            if (string.IsNullOrEmpty(personOperationParameterModel.Person.Name))
            {
                return (personOperationParameterModel.Person, 404, "Name kann nicht null oder leer sein.");
            }
            Person? existingPerson = (from p in unitOfWork.PersonRepository.Get()
                                      select p).Where(x => x.Name != null && x.Name.Equals(personOperationParameterModel.Person.Name)).FirstOrDefault();
            if (existingPerson != null)
            {
                return (existingPerson, 303, "Person already exists.");
            }
            else
            {
                Person newPerson = new() { Name = personOperationParameterModel.Person.Name };
                if (!string.IsNullOrEmpty(personOperationParameterModel.Person.Pseudonym))
                {
                    newPerson.Pseudonym = personOperationParameterModel.Person.Pseudonym;
                }
                if (!string.IsNullOrEmpty(personOperationParameterModel.Person.PersonDescription))
                {
                    newPerson.PersonDescription = personOperationParameterModel.Person.PersonDescription;
                }
                if (!string.IsNullOrEmpty(personOperationParameterModel.Person.Signature))
                {
                    newPerson.Signature = personOperationParameterModel.Person.Signature;
                }
                newPerson.BirthYear = personOperationParameterModel.Person.BirthYear;
                newPerson.DeathYear = personOperationParameterModel.Person.DeathYear;
                newPerson = unitOfWork.PersonRepository.Insert(newPerson);
                unitOfWork.Save();
                return (newPerson, 201, "Person erfolgreich erstellt.");
            }
        }

        public (Person Person, int StatusCode, string StatusMessage) Edit(PersonOperationParameterModel personOperationParameterModel)
        {
            if (string.IsNullOrEmpty(personOperationParameterModel.Person.Name))
            {
                return (personOperationParameterModel.Person, 404, "Epochenname fehlt.");
            }

            Person? existingPerson = (from p in unitOfWork.PersonRepository.Get()
                                      select p).Where(x => x.PersonID == personOperationParameterModel.Person.PersonID).FirstOrDefault();
            if (existingPerson == null)
            {
                return (personOperationParameterModel.Person, 404, "Person not found.");
            }

            existingPerson.Name = personOperationParameterModel.Person.Name;
            existingPerson.Pseudonym = personOperationParameterModel.Person.Pseudonym;
            existingPerson.PersonDescription = personOperationParameterModel.Person.PersonDescription;
            existingPerson.Signature = personOperationParameterModel.Person.Signature;
            existingPerson.BirthYear = personOperationParameterModel.Person.BirthYear;
            existingPerson.DeathYear = personOperationParameterModel.Person.DeathYear;
            unitOfWork.Save();
            return (existingPerson, 200, "Person successfully updated.");
        }

        public List<PersonOperationParameterModel> GetWithPredicates(PersonSearchParameterModel personSearchParameterModel)
        {
            IEnumerable<Person> person = unitOfWork.PersonRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Person>(personSearchParameterModel),
                includeProperties: "City");

            return [.. from p in person
                       select new PersonOperationParameterModel {
                           Person = p
                       }];
        }
    }
}
