using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using System.Diagnostics;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public partial class HomeController(IWebHostEnvironment hostEnvironment
        ) : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;

        public IActionResult Frontpage(string statusMessage, string pathFrontside, string pathBackside, List<string> buildingList, List<string> cities
            , List<string> adresses, string stamp, string text)
        {
            // Move this to a scheduled task, when a server was booked
            string[] files = Directory.GetFiles(Path.Combine(_hostEnvironment.WebRootPath, "images/Zwischenablage"));
            DateTime dtNow = DateTime.Now;
            foreach (var file in files)
            {
                DateTime dtFile = System.IO.File.GetLastAccessTime(file);
                if (dtFile.AddDays(2) < dtNow)
                    System.IO.File.Delete(file);
            }

            ViewData["StatusMessage"] = statusMessage;
            ViewData["PathFrontside"] = pathFrontside;
            ViewData["PathBackside"] = pathBackside;
            ViewData["Building"] = buildingList;
            ViewData["City"] = cities;
            ViewData["Address"] = adresses;
            ViewData["Stamp"] = stamp;
            ViewData["Text"] = text;

            return View();
        }

        //public ActionResult SpellChecker()
        //{
        //    string germanWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "wordlist-german.txt");
        //    List<string> germanWords = [.. File.ReadAllLines(germanWordlist)];
        //    var resultSpellCheck = SpellCheck.WordProbabilityTupleList("An", germanWords);
        //}

        public ActionResult Pricing()
        {
            return View();
        }

        public ActionResult PrivacyImprint()
        {
            return View();
        }

        public ActionResult TermsAndConditions()
        {
            return View();
        }

        public ActionResult ReferenceList()
        {
            return View();
        }

        public ActionResult Disclaimer()
        {
            return View();
        }

        public ActionResult Documentation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult OnPostWithdrawConsent()
        {
            IFeatureCollection features = HttpContext.Features;
            features?.Get<ITrackingConsentFeature>()?.WithdrawConsent();
            return RedirectToAction("FrontPage", "Home");
        }
        public IActionResult OnPostAcceptConsent()
        {
            IFeatureCollection features = HttpContext.Features;
            features?.Get<ITrackingConsentFeature>()?.GrantConsent();
            return RedirectToAction("FrontPage", "Home");
        }

        //[GeneratedRegex(@"[-]")]
        //private static partial Regex RegexConjunctionWord();
    }
}
