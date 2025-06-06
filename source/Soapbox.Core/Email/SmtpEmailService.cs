namespace Soapbox.Application.Email;

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.Extensions.Options;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;

[Injectable<IEmailService>]
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly IEmailRenderer _renderer;

    public SmtpEmailService(IOptionsSnapshot<SmtpSettings> options, IEmailRenderer renderer)
    {
        _settings = options.Value;
        _renderer = renderer;
    }

    public async Task<Result> SendEmailAsync<TModel>(string recipient, string subject, TModel model)
    {
        var htmlBody = await _renderer.Render(typeof(TModel).Name, model);

        try
        {
            using var client = GetClient();
            var mailMessage = new MailMessage(_settings.Sender, recipient, subject, htmlBody) { IsBodyHtml = true };

            await client.SendMailAsync(mailMessage);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Error.Unknown(e.Message);
        }
    }

    private SmtpClient GetClient() => new(_settings.Host, _settings.Port)
    {
        Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
        EnableSsl = _settings.EnableSsl,
    };
}
