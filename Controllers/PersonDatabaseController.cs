using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Services.Processes;

namespace Sammlerplattform.Controllers
{
    public class PersonDatabaseController (IProcessPerson processPerson) : Controller
    {
        public ActionResult Index(string statusMessage, PersonSearchParameterModel personSearchParameterModel)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View(processPerson.GetWithPredicates(personSearchParameterModel));
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public ActionResult CreateSubmit(PersonOperationParameterModel personOperationParameterModel)
        {
            (Person _, int _, string statusMessage) = processPerson.Create(personOperationParameterModel);

            return RedirectToAction(nameof(Index), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            PersonOperationParameterModel? personOperationParameterModel = (from p in processPerson.GetWithPredicates(new PersonSearchParameterModel { PersonID = [id] })
                              select p).FirstOrDefault();

            return View(personOperationParameterModel);
        }
        public ActionResult EditSubmit(PersonOperationParameterModel personOperationParameterModel)
        {
            (Person person, int _, string statusMessage) = processPerson.Edit(personOperationParameterModel);

            return RedirectToAction(nameof(Edit), new {statusMessage, id = person.PersonID });
        }
    }
}
