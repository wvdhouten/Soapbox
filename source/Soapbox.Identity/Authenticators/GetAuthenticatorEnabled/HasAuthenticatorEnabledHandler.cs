namespace Soapbox.Identity.Authenticators.GetAuthenticatorEnabled;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class HasAuthenticatorEnabledHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public HasAuthenticatorEnabledHandler(IHttpContextAccessor contextAccessor, TransactionalUserManager<SoapboxUser> userManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
    }

    public async Task<Result<bool>> HasAuthenticatorEnabled()
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var result = await _userManager.GetAuthenticatorKeyAsync(userResult.Value);
        return result is not null;
    }
}
