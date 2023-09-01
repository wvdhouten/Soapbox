namespace Soapbox.Core.Email
{
    using System.Threading.Tasks;

    public interface IEmailService
    {
        Task SendEmailAsync<TModel>(string recipient, string subject, TModel model);
    }
}
