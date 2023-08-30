namespace Soapbox.Core.Email
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Email.Abstractions;

    public class SmtpEmailClient : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly IEmailRenderer _renderer;

        public SmtpEmailClient(IOptionsSnapshot<SmtpSettings> options, IEmailRenderer renderer)
        {
            _settings = options.Value;
            _renderer = renderer;
        }

        public async Task SendEmailAsync<T>(string recipient, string subject, T model)
        {
            var htmlBody = await _renderer.Render(typeof(T).Name, model);

            using var client = GetClient();
            var mailMessage = new MailMessage(_settings.Sender, recipient, subject, htmlBody)
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
