using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Services.Processes;
using System.Diagnostics;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public partial class HomeController(IProcessProduct processBrick) : Controller
    {

        public IActionResult Frontpage(BrickSearchParameterModel searchParameterModel)
        {
            List<BrickOperationParameterModel> brickOperationParameterModel = processBrick.GetWithPredicates(searchParameterModel);

            return View(brickOperationParameterModel);
        }

        public ActionResult PrivacyImprint()
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
            return RedirectToAction(nameof(Frontpage));
        }
        public IActionResult OnPostAcceptConsent()
        {
            IFeatureCollection features = HttpContext.Features;
            features?.Get<ITrackingConsentFeature>()?.GrantConsent();
            return RedirectToAction(nameof(Frontpage));
        }
    }
}
