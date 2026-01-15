using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;

namespace Sammlerplattform.Controllers
{
    public class ImprovementSuggestionsController(IProcessImprovementSuggestions processImprovementSuggestions,
            UserManager<UsingIdentityUser> userManager) : Controller
    {
        [HandleStatus]
        public ActionResult Index(TopicSearchParameterModel searchParameterModel)
        {
            ViewData["UserID"] = userManager.GetUserId(User);

            return View(processImprovementSuggestions.GetWithPredicate(searchParameterModel));
        }

        [HandleStatus]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSubmit(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int _) = processImprovementSuggestions
                .Insert(topic.Title, topic.Content, userManager.GetUserId(User) ?? throw new NullReferenceException());
            return RedirectToAction(nameof(Index), new { status = new Status { StatusCode = statusCode, StatusMessage = statusMessage } });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Topic? topic = processImprovementSuggestions.GetWithPredicate(new TopicSearchParameterModel { Id = [id] }).FirstOrDefault();

            return topic == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ImprovementSuggestion_NotFound" })
                : View(topic);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSubmit(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int statusCode, string statusMessage, int id ) = processImprovementSuggestions
                .Update(topic.Id, topic.Title, topic.Content);
            if (statusCode == 200)
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
            else
                return RedirectToAction(nameof(Edit), new { id, status = new Status { StatusMessage = statusMessage } });
        }

        [HandleStatus]
        public ActionResult Delete(int id)
        {
            Topic? topic = processImprovementSuggestions.GetWithPredicate(new TopicSearchParameterModel { Id = [id] }).FirstOrDefault();

            return topic == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ImprovementSuggestion_NotFound" })
                : View(topic);
        }
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSubmit(int id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_InvalidModelState" });
            }
            (int _, string statusMessage) = processImprovementSuggestions.Delete(id);
            return RedirectToAction(nameof(Index), new { statusMessage });
        }
    }
}
