using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using System.Diagnostics;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public class HomeController(IProcessCollectionItemEntity processCollectionItemEntity,
        IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        public IActionResult Frontpage(Status status, CollectionItemSearchParameterModel itemSearchParameterModel, int topK)
        {
            HandleStatus(status);

            var entities = processCollectionItemEntity.GetTraditionalTextSearch(itemSearchParameterModel);
            int maxRows = entities.Count;
            if (maxRows > 24)
            {
                if (topK == 0)
                {
                    topK = 25;
                }
                else
                {
                    int lastAmountRows = topK;
                    ViewData["lastAmountRows"] = lastAmountRows;

                    topK += 15;
                }

                ViewData["TopK"] = topK;
            }
            else
            {
                topK = maxRows;
            }
            ViewData["maxRows"] = maxRows;
            entities = [.. entities.Take(topK)];

            return View(entities);
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = stringLocalizer[status.Message].Value;
                ViewData["StatusCode"] = status.Code;
            }
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
