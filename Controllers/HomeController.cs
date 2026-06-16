using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using System.Diagnostics;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public class HomeController(IProcessCollectionItemEntity processCollectionItemEntity) : Controller
    {
        [HandleStatus]
        public async Task<IActionResult> Frontpage(CollectionItemSearchParameterModel itemSearchParameterModel, int topK)
        {
            List<CollectionItemDisplayDTO> entities = await processCollectionItemEntity.GetWithVector(itemSearchParameterModel);

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

        public ActionResult Details(int entityId)
        {
            CollectionItemDisplayDTO? entity = processCollectionItemEntity.GetWithPredicates(new CollectionItemSearchParameterModel { CollectionItemEntityID = [entityId] }).FirstOrDefault();

            return entity == null
                ? RedirectToAction(nameof(Frontpage), new Status { StatusCode = 404, StatusMessage = "Status_EntityNotFound" })
                : View(entity);
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
