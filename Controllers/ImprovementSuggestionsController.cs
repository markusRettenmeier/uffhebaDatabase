using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;

namespace Sammlerplattform.Controllers
{
    //[Authorize]
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
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_ModelState_Invalid" });
            }
            (int statusCode, string statusMessage, int _) = processImprovementSuggestions
                .Insert(topic.Title, topic.Content, userManager.GetUserId(User) ?? throw new NullReferenceException());
            return RedirectToAction(nameof(Index), new { status = new Status { StatusCode = statusCode, StatusMessage = statusMessage } });
        }

        [HandleStatus]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Topic? topic = processImprovementSuggestions.GetWithPredicate(new TopicSearchParameterModel { Id = [id] }).FirstOrDefault();

            return topic == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ImprovementSuggestion_NotFound" })
                : View(topic);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_ModelState_Invalid" });
            }
            (int statusCode, string statusMessage, int id ) = processImprovementSuggestions
                .Update(topic.Id, topic.Title, topic.Content);
            if (statusCode == 200)
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
            else
                return RedirectToAction(nameof(Edit), new { id, status = new Status { StatusMessage = statusMessage } });
        }

        //[HandleStatus]
        //[HttpGet]
        //public ActionResult Delete(int id)
        //{
        //    Topic? topic = processImprovementSuggestions.GetWithPredicate(new TopicSearchParameterModel { Id = [id] }).FirstOrDefault();

        //    return topic == null
        //        ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ImprovementSuggestion_NotFound" })
        //        : View(topic);
        //}
        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult Delete(int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction(nameof(Index), new { statusMessage = "Error_ModelState_Invalid" });
        //    }
        //    (int _, string statusMessage) = processImprovementSuggestions.Delete(id);
        //    return RedirectToAction(nameof(Index), new { statusMessage });
        //}
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var topic = processImprovementSuggestions.GetWithPredicate(new TopicSearchParameterModel { Id = [id] }).FirstOrDefault();
            return topic == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_ImprovementSuggestion_NotFound" })
                : View(topic);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });

            (int _, string statusMessage) = processImprovementSuggestions.Delete(id);

            return RedirectToAction(nameof(Index),
                new { statusMessage });
        }
    }
}
