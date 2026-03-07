using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Sammlerplattform.Resources;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Sammlerplattform.Services.EMail
{
    public class EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       IWebHostEnvironment hostEnvironment,
        IStringLocalizer<SharedResources> stringLocalizer,
        IHtmlLocalizer<SharedResources> htmlLocalizer,
        ITrackEventsCSV trackEvents) : IEmailSender
    {
        public AuthMessageSenderOptions Options { get; } = optionsAccessor.Value;

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string? sendGridKey = Options.SendGridKey;
            if (string.IsNullOrEmpty(sendGridKey))
            {
                throw new Exception("Null SendGridKey");
            }
            await Execute(sendGridKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            SendGridClient client = new(apiKey);
            EmailAddress fromEmail = new("noreply@uffheba.software", "uffheba");
            EmailAddress sendGridToEmail = new(toEmail);
            string plainTextContent = stringLocalizer["Message_Greetings_String"] +
                            message +
                            stringLocalizer["Message_Greetings_HTML"];
            string htmlContent = htmlLocalizer["Message_GeneralText_Html"] +
                            message +
                            htmlLocalizer["Message_GeneralText_Html"];
            SendGridMessage msg = MailHelper.CreateSingleEmail(fromEmail, sendGridToEmail, subject, plainTextContent, htmlContent);
            if (subject == "Abonnement abgeschlossen")
            {
                AttachFile(msg, "Verbraucher-Widerrufsbelehrung_für_Dienstleistungen.pdf", Path.Combine(hostEnvironment.WebRootPath, "doc\\Verbraucher-Widerrufsbelehrung_für_Dienstleistungen.pdf"));
                AttachFile(msg, "AGB.pdf", Path.Combine(hostEnvironment.WebRootPath, "doc\\AGB.pdf"));
                AttachFile(msg, "Preisbildung.pdf", Path.Combine(hostEnvironment.WebRootPath, "doc\\Preisbildung.pdf"));
            }

            Response response = await client.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                trackEvents.TrackInfo("Email sent to {toEmail} with subject {subject}.", new Dictionary<string, object>
                {
                    { "toEmail", toEmail },
                    { "subject", subject }
                });
            }
            else
            {
                trackEvents.TrackEmailResponse(response, toEmail, "Failure Email to {toEmail} with error {response.StatusCode}.");
            }
        }

        private static void AttachFile(SendGridMessage msg, string name, string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string file = Convert.ToBase64String(bytes);
            msg.AddAttachment(name, file);
        }
    }
}
