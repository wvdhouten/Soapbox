namespace Soapbox.Core.Email
{
    using System.Threading.Tasks;

    public interface IEmailService
    {
        Task SendEmailAsync<T>(string recipient, string subject, T model);
    }
}
