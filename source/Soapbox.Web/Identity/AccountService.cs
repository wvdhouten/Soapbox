namespace Soapbox.Web.Identity;

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Users;
using Soapbox.Web.Identity.Managers;

public class AccountService
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly ILogger<AccountService> _logger;

    public AccountService(TransactionalUserManager<SoapboxUser> userManager, ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<SoapboxUser?> FindUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        return user;
    }

    public async Task<IdentityResult> UpdateUserAsync(SoapboxUser user)
    {
        var existing = await _userManager.FindByIdAsync(user.Id)
            ?? throw new InvalidOperationException($"User with ID '{user.Id}' not found.");

        existing.UserName = user.UserName;
        existing.Email = user.Email;
        existing.DisplayName = user.DisplayName;
        existing.Role = user.Role;

        var result = await _userManager.UpdateAsync(existing);
        await _userManager.CommitAsync();

        return result;
    }

    public async Task<string> GeneratePasswordResetCode(SoapboxUser user)
    {
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    }
}
