namespace Soapbox.Domain.Email;

using System.Threading.Tasks;

public interface IEmailService
{
    public Task SendEmailAsync<TModel>(string recipient, string subject, TModel model);
}
