namespace Soapbox.Identity;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

public class AccountService
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public AccountService(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
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
}
