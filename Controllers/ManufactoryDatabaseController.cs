using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Services.Processes;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class ManufactoryDatabaseController(IProcessManufactory processManufactory) : Controller
    {
        public IActionResult AdministerCollectionManufactory(string statusMessage, ManufactorySearchParameterModel manufactorySearchParameterModel)
        {
            List<Manufactory> manufactorySelect = [.. (from m in processManufactory.GetManufactoryWithPredicates(manufactorySearchParameterModel)
                                                   select m).Select(x => x.Manufactory)];

            ViewData["StatusMessage"] = statusMessage;

            return View(manufactorySelect);
        }

        public ActionResult CreateManufactory(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public IActionResult CreateManufactorySubmit(ManufactoryOperationParameterModel model)
        {
            (Manufactory _, int _, string statusMessage) = processManufactory.CreateManufactory(model);

            return RedirectToAction(nameof(CreateManufactory), new { statusMessage });
        }

        public ActionResult EditManufactory(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            ManufactorySearchParameterModel manufactorySearch = new();
            manufactorySearch.ManufactoryID.Add(id);
            ManufactoryOperationParameterModel parameterModel = processManufactory.GetManufactoryWithPredicates(manufactorySearch).First();

            return View(parameterModel);
        }

        public ActionResult EditManufactorySubmit(ManufactoryOperationParameterModel manufactoryParameterModel)
        {
            (Manufactory manufactory, int _, string statusMessage) = processManufactory.EditManufactory(manufactoryParameterModel);

            return RedirectToAction(nameof(EditManufactory), new { statusMessage, id = manufactory.ManufactoryID });
        }
    }
}