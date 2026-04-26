using ApplePaySimulation.Services.Abstracts;
using System.Net.Mail;
using System.Net;
using ApplePaySimulation.Models.SettingsModels;

namespace ApplePaySimulation.Services.Implementations
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;


        public EmailSenderService(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, EmailSettings emailSettings)
        {

            await SendViaSmtp(email, subject, htmlMessage, emailSettings);
        }

        private async Task SendViaSmtp(string email, string subject, string htmlMessage, EmailSettings emailSettings)
        {
            string fromMail = emailSettings.SmtpUser;
            string fromPassword = emailSettings.SmtpPassword; // You must fill this in with an App Password

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(email));
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = emailSettings.SmtpPort,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = emailSettings.SmtpUseSSL,

            };

            smtpClient.Send(message);
        }
    }
}
