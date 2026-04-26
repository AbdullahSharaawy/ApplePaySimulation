using ApplePaySimulation.Models.SettingsModels;

namespace ApplePaySimulation.Services.Abstracts
{
    public interface IEmailSenderService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage, EmailSettings emailSettings);

    }
}
