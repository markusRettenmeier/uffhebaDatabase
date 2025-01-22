using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.PersonDatabase;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    public class PersonDatabaseController : Controller
    {
    }

    public interface IProcessPerson
    {
        (Person person, int statuscode, string message) CreatePerson(PersonOperationParameterModel personOperationParameterModel);
        (Person person, int statuscode, string message) EditPerson(PersonOperationParameterModel personOperationParameterModel);
        IEnumerable<Person> GetPersonWithPredicate(PersonSearchParameterModel personSearchParameterModel);
        PersonSearchParameterModel PersonParametersOperationToSearch(PersonOperationParameterModel personOperation);
    }
    public class PersonProcessor(IUnitOfWork unitOfWork, ILogger<PersonProcessor> logger) : IProcessPerson
    {
        public PersonSearchParameterModel PersonParametersOperationToSearch(PersonOperationParameterModel personOperation)
        {
            PersonSearchParameterModel personSearch = new();
            personSearch.SearchPersonID.Add(personOperation.Person.Person_ID);
            if (personOperation.Person.Name != null)
                personSearch.SearchName.Add(personOperation.Person.Name);
            if(personOperation.Person.Pseudonym != null)
                personSearch.SearchPseudonym.Add(personOperation.Person.Pseudonym);
            if(personOperation.Person.PersonSignature != null)
                personSearch.SearchSignature.Add(personOperation.Person.PersonSignature);
            if(personOperation.Person.BirthYear != null)
                personSearch.SearchBirthYear.Add(personOperation.Person.BirthYear.Value);
            if(personOperation.Person.DeathYear != null)
                personSearch.SearchDeathYear.Add(personOperation.Person.DeathYear.Value);

            return personSearch;
        }

        public (Person person, int statuscode, string message) CreatePerson(PersonOperationParameterModel personOperationParameterModel)
        {
            if (personOperationParameterModel.Person.Equals(string.Empty))
            {
                return (new() { Name = string.Empty }, 412, "Name angeben.");
            }

            IEnumerable<Person> personIEnumerable = GetPersonWithPredicate(PersonParametersOperationToSearch(personOperationParameterModel));
            Person? personExists = personIEnumerable.FirstOrDefault();
            if(personExists != null)
            {
                return (personExists, 302, "Eintrag existiert bereits.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Person newPerson = unitOfWork.PersonRepository.Insert(personOperationParameterModel.Person);
                unitOfWork.Save();

                ConnectCityToPerson(personOperationParameterModel.CityIDList, newPerson);
                ConnectPrizeToPerson(personOperationParameterModel.PrizeList, newPerson);
                ConnectProfessionToPerson(personOperationParameterModel.ProfessionList, newPerson);

                scope.Complete();
                return (newPerson, 201, "Person wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new() { Name = string.Empty }, 500, $"Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten: {ex}.");
            }
        }

        public (Person person, int statuscode, string message) EditPerson(PersonOperationParameterModel personOperationParameterModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Person> GetPersonWithPredicate(PersonSearchParameterModel personSearchParameter)
        {
            ExpressionStarter<Person> predicate = PredicateBuilder.New<Person>();
            IEnumerable<Person> personExists = from p in unitOfWork.PersonRepository.Get(includeProperties: "City,PrizeICollection,BrickEntityBrickmakerICollection,BrickEntityOwnerICollection,ProfessionICollection")
                                               select p;

            predicate = GenericClasses.GenericLambdas.CreateLambdaSpanIntJoin<Person>("Person", "Person_ID", personSearchParameter.SearchPersonID);
            predicate = GenericClasses.GenericLambdas.CreateLambdaStringContainsJoin<Person>("Person", "Name", personSearchParameter.SearchName);
            predicate = GenericClasses.GenericLambdas.CreateLambdaStringContainsJoin<Person>("Person", "Pseudonym", personSearchParameter.SearchPseudonym);
            predicate = GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<Person>("Person", "PersonSignature", personSearchParameter.SearchSignature);

            if (predicate.IsStarted)
            {
                personExists = personExists.Where(predicate);
            }

            return personExists;
        }

        private void ConnectCityToPerson(List<int> cityList, Person person)
        {
            foreach (int city in cityList)
            {
                if (city > 0)
                {
                    City? existingCity = (from c in unitOfWork.CityRepository.Get()
                                          where c.City_ID == city
                                          select c).FirstOrDefault();
                    if (existingCity != null)
                    {
                        unitOfWork.PersonRepository.SetForeignKey(person, p => p.City_ID, existingCity.City_ID);
                        unitOfWork.CityRepository.AddMemberToCollection(existingCity, c => c.PersonICollection, person);
                        unitOfWork.Save();
                    }
                    else
                    {
                        logger.LogWarning("Eingegebener Ort {city} nicht verfügbar.", city);
                    }
                }
            }
        }
        private void ConnectPrizeToPerson(List<string> prizeList, Person person)
        {
            foreach (string prizeName in prizeList)
            {
                if (!string.IsNullOrEmpty(prizeName))
                {
                    Prize? existingPrize = (from p in unitOfWork.PrizeRepository.Get()
                                          where p.Name.Equals(prizeName)
                                          select p).FirstOrDefault();
                    if (existingPrize != null)
                    {
                        unitOfWork.PersonRepository.AddMemberToCollection(person, p => p.PrizeICollection, existingPrize);
                        unitOfWork.PrizeRepository.AddMemberToCollection(existingPrize, c => c.PersonICollection, person);
                        unitOfWork.Save();
                    }
                    else
                    {
                        logger.LogWarning("Eingegebener Preis {prizeName} nicht verfügbar.", prizeName);
                    }
                }
            }
        }
        private void ConnectProfessionToPerson(List<string> professionList, Person person)
        {
            foreach (string professionName in professionList)
            {
                if (!string.IsNullOrEmpty(professionName))
                {
                    Profession? existingProfession = (from p in unitOfWork.ProfessionRepository.Get()
                                          where p.Name == professionName
                                          select p).FirstOrDefault();
                    if (existingProfession != null)
                    {
                        unitOfWork.PersonRepository.AddMemberToCollection(person, p => p.ProfessionICollection, existingProfession);
                        unitOfWork.ProfessionRepository.AddMemberToCollection(existingProfession, c => c.PersonICollection, person);
                        unitOfWork.Save();
                    }
                    else
                    {
                        logger.LogWarning("Eingegebener Preis {professionName} nicht verfügbar.", professionName);
                    }
                }
            }
        }
    }
}
