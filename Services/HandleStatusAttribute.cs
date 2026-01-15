using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models;
using Sammlerplattform.Resources;

namespace Sammlerplattform.Services
{
    public class HandleStatusAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                // Status aus Query-Parametern konstruieren
                var status = new Status
                {
                    StatusCode = GetIntFromQuery(context, "statusCode"),
                    StatusMessage = GetStringFromQuery(context, "statusMessage")
                };

                if (!string.IsNullOrEmpty(status.StatusMessage))
                {
                    var stringLocalizer = context.HttpContext.RequestServices
                        .GetRequiredService<IStringLocalizer<SharedResources>>();

                    string statusMessage = stringLocalizer[status.StatusMessage];
                    controller.ViewData["StatusMessage"] = statusMessage;
                    controller.ViewData["StatusCode"] = status.StatusCode;
                }
            }
        }

        private static int GetIntFromQuery(ActionExecutingContext context, string key)
        {
            if (context.HttpContext.Request.Query.TryGetValue(key, out var value) &&
                int.TryParse(value, out var intValue))
            {
                return intValue;
            }
            return 0;
        }

        private static string? GetStringFromQuery(ActionExecutingContext context, string key)
        {
            return context.HttpContext.Request.Query.TryGetValue(key, out var value)
                ? value.ToString()
                : null;
        }
    }
}
