namespace Soapbox.Identity.Mfa.Get2faEnabled;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class GetMfaEnabledHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public GetMfaEnabledHandler(IHttpContextAccessor contextAccessor, TransactionalUserManager<SoapboxUser> userManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
    }

    public async Task<Result<bool>> GetMfaEnabledAsync()
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        return await _userManager.GetTwoFactorEnabledAsync(userResult.Value);
    }
}
