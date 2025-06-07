namespace Soapbox.Domain.Email;

using Soapbox.Domain.Results;
using System.Threading.Tasks;

public interface IEmailService
{
    public Task<Result> SendEmailAsync<TModel>(string recipient, string subject, TModel model);
}
