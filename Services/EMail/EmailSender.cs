using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Sammlerplattform.Services.EMail
{
    public class EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger,
                       IWebHostEnvironment hostEnvironment) : IEmailSender
    {
        private readonly ILogger _logger = logger;

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
            EmailAddress fromEmail = new("noreply@sammlerdb.de", "sammlerdb");
            EmailAddress sendGridToEmail = new(toEmail);
            string plainTextContent = "Guten Tag" +
                            "<br />" + message +
                            "<br />Dies ist eine automatisch generierte Mail. Bei Fragen, wenden Sie sich bitte an service@uffheba.software." +
                            "<br /><br />Viele Grüße";
            string htmlContent = "Guten Tag" +
                            "<br />" + message +
                            "<br />Dies ist eine automatisch generierte Mail. Bei Fragen, wenden Sie sich bitte an service@uffheba.software." +
                            "<br />Viele Grüße";
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
                _logger.LogInformation("Email to {toEmail} queued successfully!", toEmail);
            }
            else
            {
                _logger.LogError("Failure Email to {toEmail} with error {response.StatusCode}.", toEmail, response.StatusCode);
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
