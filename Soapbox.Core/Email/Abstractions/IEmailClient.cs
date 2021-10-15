namespace Soapbox.Core.Email
{
    using System.Threading.Tasks;

    public interface IEmailClient
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
