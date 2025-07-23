using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Services.GenericClasses;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessPerson
    {
        (Person Person, int Statuscode, string StatusMessage) Create(PersonOperationParameterModel personOperationParameterModel);
        (Person Person, int statusCode, string statusMessage) Edit(PersonOperationParameterModel personOperationParameterModel);
        List<PersonOperationParameterModel> GetWithPredicates(PersonSearchParameterModel personSearchParameterModel);
        PersonSearchParameterModel ParametersOperationToSearch(PersonOperationParameterModel personOperationParameterM);
    }

    public class PersonProcessor(IUnitOfWork unitOfWork) : IProcessPerson
    {
        public (Person Person, int Statuscode, string StatusMessage) Create(PersonOperationParameterModel personOperationParameterModel)
        {
            throw new NotImplementedException();
        }

        public (Person Person, int statusCode, string statusMessage) Edit(PersonOperationParameterModel personOperationParameterModel)
        {
            throw new NotImplementedException();
        }

        public List<PersonOperationParameterModel> GetWithPredicates(PersonSearchParameterModel personSearchParameterModel)
        {
            IEnumerable<Person> person = unitOfWork.PersonRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Person>(personSearchParameterModel),
                includeProperties: "City");

            return [.. from p in person
                       select new PersonOperationParameterModel {
                           Person = p,
                           City = p.City ?? new()
                       }];
        }

        public PersonSearchParameterModel ParametersOperationToSearch(PersonOperationParameterModel personOperationParameterM)
        {
            throw new NotImplementedException();
        }
    }
}
