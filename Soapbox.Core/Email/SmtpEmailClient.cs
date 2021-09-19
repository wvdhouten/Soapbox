namespace Soapbox.Core.Email
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Email.Abstractions;

    public class SmtpEmailClient : IEmailClient
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailClient(IOptionsSnapshot<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = GetClient();
            var mailMessage = new MailMessage(_settings.Sender, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }

        private SmtpClient GetClient()
        {
            return new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl,
            };
        }
    }
}
