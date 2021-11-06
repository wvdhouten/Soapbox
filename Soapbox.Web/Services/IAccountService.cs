namespace Soapbox.Web.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Soapbox.Models;

    public interface IAccountService
    {
        Task<SoapboxUser> FindUserByIdAsync(string id);

        Task<Dictionary<string,string>> GetPersonalDataAsync(SoapboxUser user);

        Task<string> GeneratePasswordResetCode(SoapboxUser user);

        Task DeleteUserAsync(SoapboxUser user);

        Task<IdentityResult> UpdateUserAsync(SoapboxUser user);
    }
}
