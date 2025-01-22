using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class PaymentController(UserManager<UsingIdentityUser> userManager,
                                   IConfiguration configuration, ILogger<PaymentController> logger, IEmailSender emailSender) : Controller
    {
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;

        public async Task<ActionResult> Checkout(string email, string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.FindByEmailAsync(email);
            int subDiskSpaceState = 0;
            string subDiskSpace_Id = string.Empty;
            DateTime diskspaceDisclaimerDateTime = new();
            int subAnalysisState = 0;
            string subAnalysis_Id = string.Empty;
            DateTime analysisDisclaimerDateTime = new();
            string? lookupDiskSpace = configuration.GetValue<string>("StripeLookupDiskSpace");
            string? lookupAnalysistool = configuration.GetValue<string>("StripeLookupAnalysisTool");

            if (user != null && user.StripeCustomer_ID != null)
            {
                SubscriptionListOptions options = new() { Limit = 3, Customer = user.StripeCustomer_ID };
                SubscriptionService service = new();
                try
                {
                    StripeList<Subscription> subscriptions = service.List(options);

                    foreach (Subscription sub in subscriptions)
                    {
                        foreach (SubscriptionItem? subData in sub.Items.Data)
                        {
                            if (sub.Items.Data[0].Price.LookupKey == lookupDiskSpace)
                            {
                                subDiskSpace_Id = sub.Id;
                                diskspaceDisclaimerDateTime = sub.Created;
                                if (sub.CancelAtPeriodEnd == true)
                                {
                                    //gecanceltes Abo
                                    subDiskSpaceState = 2;
                                }
                                else
                                {
                                    //laufendes Abo
                                    subDiskSpaceState = 1;
                                }
                            }
                            else if (sub.Items.Data[0].Price.LookupKey == lookupAnalysistool)
                            {
                                    subAnalysis_Id = sub.Id;
                                    analysisDisclaimerDateTime = sub.Created;
                                    if (sub.CancelAtPeriodEnd == true)
                                    {
                                        //gecanceltes Abo
                                        subAnalysisState = 2;
                                    }
                                    else
                                    {
                                        //laufendes Abo
                                        subAnalysisState = 1;
                                    }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("CheckoutSubmit mit {ex.Message}", ex.Message);
                }
            }

            ViewData["EMail"] = email;
            ViewData["StatusMessage"] = statusMessage;
            ViewData["DiskspaceState"] = subDiskSpaceState;
            ViewData["DiskspaceId"] = subDiskSpace_Id;
            ViewData["DiskspaceDisclaimerTimestamp"] = diskspaceDisclaimerDateTime.AddDays(15);
            ViewData["LookupDiskSpace"] = lookupDiskSpace;
            ViewData["AnalysisState"] = subAnalysisState;
            ViewData["AnalysisId"] = subAnalysis_Id;
            ViewData["AnalysisDisclaimerDate"] = analysisDisclaimerDateTime.AddDays(15);
            ViewData["LookupAnalysistool"] = lookupAnalysistool;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CheckoutSubmit(string email, string lookup_key)
        {
            HttpRequest request = HttpContext.Request;
            string domain = $"{request.Scheme}://{request.Host}";

            PriceListOptions priceOptions = new()
            {
                LookupKeys = [lookup_key]
            };
            PriceService priceService = new();

            try
            {
                StripeList<Price> prices = priceService.List(priceOptions);

                SessionCreateOptions options = new();
                UsingIdentityUser user = await _userManager.FindByEmailAsync(email) ?? throw new NullReferenceException();

                int daysThisMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                int daysNextMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month + 1);
                int daysPassedThisMonth = DateTime.Now.Day;
                // Billing shoud happen in the middle of month so +16
                int remainingTestDays = daysThisMonth + daysNextMonth - daysPassedThisMonth + 16;

                options = user.StripeCustomer_ID != null
                    ? new SessionCreateOptions
                    {
                        LineItems =
                        [
                          new SessionLineItemOptions
                          {
                              Price = prices.Data[0].Id
                          },
                        ],
                        Customer = user.StripeCustomer_ID,
                        Mode = "subscription",
                        SubscriptionData =
                            new SessionSubscriptionDataOptions
                            {
                                TrialPeriodDays = remainingTestDays,
                            },
                        SuccessUrl = domain + "/Account/Logout?returnUrl=%2FAccount%2FLogin",
                        CancelUrl = domain + "/UserSettings/Profile?statusMessage=Cancel"
                    }
                    : new SessionCreateOptions
                    {
                        LineItems =
                        [
                          new SessionLineItemOptions
                          {
                              Price = prices.Data[0].Id
                          },
                        ],
                        CustomerEmail = user.Email,
                        Mode = "subscription",
                        SubscriptionData =
                            new SessionSubscriptionDataOptions
                            {
                                TrialPeriodDays = remainingTestDays,
                            },
                        SuccessUrl = domain + "/Account/Logout?returnUrl=%2FAccount%2FLogin",
                        CancelUrl = domain + "/UserSettings/Profile?statusMessage=Cancel"
                    };
                SessionService service = new();

                try
                {
                    Session session = service.Create(options);
                    Response.Headers.Append("Location", session.Url);

                    return new StatusCodeResult(303);
                }
                catch (Exception ex)
                {
                    logger.LogError("CheckoutSubmit mit {ex.Message}", ex.Message);

                    return RedirectToAction("Checkout", "Payment", new { email, statusMessage = ex.Message });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("CheckoutSubmit mit {ex.Message}", ex.Message);

                return RedirectToAction("Checkout", "Payment", new { email, statusMessage = ex.Message });
            }
        }

        public async Task<ActionResult> SubscriptionDisclaimerSubmit(string id, string subscriptionName, string email)
        {
            SubscriptionService service = new();
            try
            {
                _ = service.Cancel(id);
                await SendEmail(email, "Widerruf", "Hiermit haben Sie Ihr Abonnement " + subscriptionName + " erfolgreich widerrufen.");
            }
            catch (Exception ex)
            {
                logger.LogError("SubscriptionDisclaimerSubmit mit {ex.Message} und stripeCustomer_Id {id}"
                    , ex.Message, id);
            }

            return RedirectToAction("Logout", "Account", new { returnUrl = "/Account/Login" });
        }

        public async Task<ActionResult> SubscriptionCancelSubmit(string id, string subscriptionName, string email)
        {
            SubscriptionUpdateOptions options = new()
            {
                CancelAtPeriodEnd = true
            };
            SubscriptionService service = new();

            try
            {
                _ = service.Update(id, options);
                await SendEmail(email, "Abonnement beendet", $"Hiermit haben Sie Ihr Abonnement {subscriptionName} erfolgreich beendet. Das Abonnement wird noch 3 Monate weiterlaufen.");
            }
            catch (Exception ex)
            {
                logger.LogError("SubscriptionCancelSubmit mit {ex.Message} und stripeCustomer_Id {id}"
                    , ex.Message, id);
            }

            return RedirectToAction("Profile", "UserSettings", new { statusMessage = "CancelUpdate", subscription = subscriptionName });
        }

        public async Task<ActionResult> CancelSubscriptionCancelSubmit(string id, string subscriptionName, string email)
        {
            SubscriptionUpdateOptions options = new()
            {
                CancelAtPeriodEnd = false
            };
            SubscriptionService service = new();
            try
            {
                _ = service.Update(id, options);
                await SendEmail(email, "Widerruf des Aboendes", $"Hiermit haben Sie die Beendigung Ihres Abonnements {subscriptionName} widerrufen. Das Abonnement läuft weiter.");
            }
            catch (Exception ex)
            {
                logger.LogError("CancelSubscriptionCancelSubmit mit {ex.Message} und stripeCustomer_Id {id}"
                    , ex.Message, id);
            }

            return RedirectToAction("Profile", "UserSettings", new { statusMessage = "CancelCancelUpdate", subscription = subscriptionName });
        }

        public async Task SendEmail(string email, string action, string text)
        {
            await emailSender.SendEmailAsync(
                email,
                action,
                text);
        }
    }

    [Route("stripe_webhooks")]
    [ApiController]
    public class WebhookController(UserManager<UsingIdentityUser> userManager,
                                   DbIdentityContext dbIdentityContext,
                                   ILogger<WebhookController> logger,
                                   IConfiguration configuration, IEmailSender emailSender) : Controller
    {
        // TODOSammlerDb: Violates with Single Responsibility inside Events
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly DbIdentityContext _dbidentityContext = dbIdentityContext;
        private readonly ILogger<WebhookController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            // Replace this endpoint secret with your endpoint's unique secret
            // If you are testing with the CLI, find the secret by running 'stripe listen'
            // If you are using an endpoint defined with the API or dashboard, look in your webhook settings
            // at https://dashboard.stripe.com/webhooks
            string? endpointSecret = configuration.GetValue<string>("StripeWebhooks");
            if (endpointSecret == null)
            {
                _logger.LogError("Kein StripeWebhookKey");
            }

            string? lookupDiskSpace = configuration.GetValue<string>("StripeLookupDiskSpace");
            string? lookupAnalysistool = configuration.GetValue<string>("StripeLookupAnalysisTool");

            try
            {
                Event stripeEvent = EventUtility.ParseEvent(json);
                Microsoft.Extensions.Primitives.StringValues signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json,
                        signatureHeader, endpointSecret);

                if (stripeEvent.Type == "customer.created")
                {
                    if (stripeEvent.Data.Object is Customer customerCreated)
                    {
                        await ConnectLocalAccountWithStripeAccount(customerCreated);
                    }
                }
                else if (stripeEvent.Type == "customer.subscription.created")
                {
                    if (stripeEvent.Data.Object is Subscription subscription)
                    {
                        await SaveSubscriptionIntoUserClaim(lookupDiskSpace, lookupAnalysistool, subscription);
                    }
                }
                else if (stripeEvent.Type == "customer.subscription.deleted")
                {
                    if (stripeEvent.Data.Object is Subscription subscription)
                    {
                        await RemoveSubscriptionFromUserClaim(subscription);
                    }
                }
                else if (stripeEvent.Type == "customer.subscription.trial_will_end")
                {
                    if (stripeEvent.Data.Object is Subscription subscription)
                    {
                        UsingIdentityUser user = (from u in _dbidentityContext.Users
                                                  where u.StripeCustomer_ID == subscription.CustomerId
                                                  select u).First();
                        if (user.Email != null)
                        {
                            await emailSender.SendEmailAsync(user.Email, "Probezeit läuft ab", "Ihre Probezeit läuft in 3 Tagen ab. Wenn Sie Ihr Abonnement nicht kündigen, dann wird dieses automatisch kostenpflichtig.");
                        }
                    }
                }
                else if (stripeEvent.Type == "invoice.created")
                {
                    if (stripeEvent.Data.Object is Invoice invoice)
                    {
                        CountUsedDiskspaceAndSendEmailWithInvoice(invoice);
                    }
                }
                else
                {
                    _logger.LogError("Unhandled event type: {stripeEvent.Type}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError("Error: {e.Message}", e.Message);
                return BadRequest();
            }

            async Task ConnectLocalAccountWithStripeAccount(Customer customerCreated)
            {
                UsingIdentityUser? user = await _userManager.FindByEmailAsync(customerCreated.Email);
                if (user != null)
                {
                    user.StripeCustomer_ID = customerCreated.Id;
                    _ = await _userManager.UpdateAsync(user);
                }
            }

            async Task SaveSubscriptionIntoUserClaim(string? lookupDiskSpace, string? lookupAnalysistool, Subscription subscription)
            {
                UsingIdentityUser user = (from u in _dbidentityContext.Users
                                          where u.StripeCustomer_ID == subscription.CustomerId
                                          select u).First();
                SubscriptionItemListOptions options = new()
                {
                    Limit = 3,
                    Subscription = subscription.Id,
                };
                SubscriptionItemService service = new();
                StripeList<SubscriptionItem> subscriptionItems = service.List(options);

                foreach (SubscriptionItem data in subscription.Items.Data)
                {
                    if (data.Price.LookupKey == lookupAnalysistool)
                    {
                        _ = await _userManager.AddClaimAsync(user, new Claim("SubscribedAnalysisTool", subscriptionItems.First().Id));
                    }
                    else if (data.Price.LookupKey == lookupDiskSpace)
                    {
                        _ = await _userManager.AddClaimAsync(user, new Claim("SubscribedDiskspace", subscriptionItems.First().Id));
                    }

                    await SendSubscriptionCreatedMail(user, data.Price.LookupKey == lookupDiskSpace ? "Speicherlatz" : "Analysetool", subscription.TrialEnd);
                }
            }

            async Task RemoveSubscriptionFromUserClaim(Subscription subscription)
            {
                var user = (from u in _dbidentityContext.Users
                            join c in _dbidentityContext.UserClaims
                            on u.Id equals c.UserId
                            where u.StripeCustomer_ID == subscription.CustomerId
                            && c.ClaimType == subscription.Items.Data[0].Price.LookupKey
                            select new { UsingIdentityUser = u, IdentityUserClaim = c }).First();
                if (user.IdentityUserClaim.ClaimValue != null)
                {
                    _ = await _userManager.RemoveClaimAsync(user.UsingIdentityUser, new Claim(subscription.Items.Data[0].Price.LookupKey, user.IdentityUserClaim.ClaimValue));
                }
            }

            void CountUsedDiskspaceAndSendEmailWithInvoice(Invoice invoice)
            {
                int countUnits = (from u in _dbidentityContext.Users
                                  where u.StripeCustomer_ID == invoice.CustomerId
                                  join e in _dbidentityContext.PostcardEntity
                                  on u.Id equals e.UsingIdentityUsers_ID
                                  select e).Count();
                countUnits /= 500;

                if (countUnits > 0)
                {
                    IdentityUserClaim<string> claims = (from u in _dbidentityContext.Users
                                                        join c in _dbidentityContext.UserClaims
                                                        on u.Id equals c.UserId
                                                        where u.StripeCustomer_ID == invoice.CustomerId
                                                        && c.ClaimType == "SubscribedDiskspace"
                                                        select c).First();

                    SubscriptionItemUsageRecordCreateOptions options = new()
                    {
                        Quantity = countUnits
                    };
                    SubscriptionItemUsageRecordService usageRecordService = new();
                    UsageRecord usageRecord = usageRecordService.Create(claims.ClaimValue, options);
                }

                InvoiceService invoiceService = new();
                _ = invoiceService.FinalizeInvoice(invoice.Id);
            }
        }

        private async Task SendSubscriptionCreatedMail(UsingIdentityUser user, string topic, DateTime? trialEnd)
        {
            string? checkOutUrl = Url.Action(
                "Checkout", "Payment",
                new { user.Email },
                protocol: Request.Scheme);
            string? termsAndConditionsUrl = Url.Action(
                "TermsAndConditions", "Home", new { }
                , protocol: Request.Scheme);
            string? disclaimerUrl = Url.Action(
                "Disclaimer", "Home", new { }
                , protocol: Request.Scheme);
            string eMailtext = "";
            if (checkOutUrl != null && termsAndConditionsUrl != null && disclaimerUrl != null)
            {
                eMailtext = $"Ihr Abonnement {topic} wurde erfolgreich abgeschlossen. Sie können das Abo einfach unter <a href='{HtmlEncoder.Default.Encode(checkOutUrl)}'>Abonnements verwalten</a> widerrufen." +
                    $"<br />Es gelten unsere <a href='{HtmlEncoder.Default.Encode(termsAndConditionsUrl)}'>AGB</a> Hier finden Sie unsere <a href='{HtmlEncoder.Default.Encode(disclaimerUrl)}'>Widerrufsbelehrung</a>.";
            }
            if (trialEnd != null)
            {
                eMailtext += $"<br />Sie können das Produkt bis {trialEnd.Value.Day}.{trialEnd.Value.Month}.{trialEnd.Value.Year} testen. Danach läuft dieses Abonnement unbefristet. Es hat <strong>keine Mindestvertragslaufzeit</strong>. Bei einer Kündigung läuft der Vertrag noch 1 Monat weiter.";
            }
            else
            {
                eMailtext += $"<br />Sie können das Produkt bis Mitte des darauffolgenden Monats testen. Danach läuft dieses Abonnement unbefristet. Es hat <strong>keine Mindestvertragslaufzeit</strong>. Bei einer Kündigung läuft der Vertrag noch 1 Monat weiter.";
            }

            eMailtext += $"<br />Anbei finden Sie die AGB, das Widerrufsrecht, die Preisbildung.";
            if (user.Email != null)
            {
                await emailSender.SendEmailAsync(user.Email, "Abonnement abgeschlossen", eMailtext);
            }
        }

        public async Task AddCustomerToUser(Customer customer)
        {
            UsingIdentityUser? user = await _userManager.FindByEmailAsync(customer.Email);
            if (user != null)
            {
                user.StripeCustomer_ID = customer.Id;
                _ = await _userManager.UpdateAsync(user);
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }

    public class PaymentService
    {
        public static void SendUsageRecordAnalysisTool(string subscriptionItemId, int quantity, ILogger logger)
        {
            SubscriptionItemUsageRecordCreateOptions options = new()
            {
                Quantity = quantity
            };
            SubscriptionItemUsageRecordService usageRecordService = new();

            try
            {
                _ = usageRecordService.Create(subscriptionItemId, options);
            }
            catch (Exception ex)
            {
                logger.LogError("SendUsageRecordAnalysisTool fehlerhaft: {ex.Message} mit subscriptionItemId: {subscriptionItemId}",
                    ex.Message, subscriptionItemId);
            }
        }

        public static long GetCurrentInvoice(string subscriptionId, string subItemId, ILogger logger)
        {
            UpcomingInvoiceOptions options = new()
            {
                Subscription = subscriptionId
            };
            InvoiceService service = new();
            try
            {
                Invoice invoice = service.Upcoming(options);
                return invoice.Subtotal;
            }
            catch (Exception ex)
            {
                logger.LogError("GetCurrentInvoice fehlerhaft: {ex.Message} mit subscriptionId: {subscriptionId}, subItemnId: {subItemId}",
                    ex.Message, subscriptionId, subItemId);
                return 0;
            }
        }

        public static string GetSubFromSubItem(string subscriptionItemId, ILogger logger)
        {
            SubscriptionItemService service = new();
            try
            {
                SubscriptionItem subItem = service.Get(subscriptionItemId);
                return subItem.Subscription;
            }
            catch (Exception ex)
            {
                logger.LogError("GetSubFromSubItem fehlerhaft: {ex.Message} mit subscriptionItemId: {subscriptionItemId}",
                    ex.Message, subscriptionItemId);
                return string.Empty;
            }
        }
    }
}