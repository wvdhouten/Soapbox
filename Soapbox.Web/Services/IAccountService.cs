namespace Soapbox.Web.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soapbox.Core.Identity;

    public interface IAccountService
    {
        Task<Dictionary<string,string>> GetPersonalDataAsync(SoapboxUser user);
        Task<string> GeneratePasswordResetCode(SoapboxUser user);
    }
}
