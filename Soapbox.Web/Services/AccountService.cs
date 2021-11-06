namespace Soapbox.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Email;
    using Soapbox.Models;

    public class AccountService : IAccountService
    {
        private readonly UserManager<SoapboxUser> _userManager;
        private readonly IEmailClient _emailClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<SoapboxUser> userManager, IEmailClient emailClient, IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _emailClient = emailClient;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        public async Task<SoapboxUser> FindUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            return user;
        }

        public async Task<IdentityResult> UpdateUserAsync(SoapboxUser user)
        {
            var existing = await _userManager.FindByIdAsync(user.Id);

            existing.UserName = user.UserName;
            existing.Email = user.Email;
            existing.DisplayName = user.DisplayName;
            existing.Role = user.Role;

            var result = await _userManager.UpdateAsync(existing);

            return result;
        }

        public async Task DeleteUserAsync(SoapboxUser user)
        {
            await _userManager.DeleteAsync(user);
        }

        public async Task<string> GeneratePasswordResetCode(SoapboxUser user)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        }

        public async Task<Dictionary<string, string>> GetPersonalDataAsync(SoapboxUser user)
        {
            _logger.LogInformation($"User with ID '{user.Id}' requested their personal data.");

            var personalData = new Dictionary<string, string>();

            var personalDataProps = typeof(SoapboxUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var property in personalDataProps)
            {
                personalData.Add(property.Name, property.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var login in logins)
            {
                personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
            }

            return personalData;
        }
    }
}
