namespace Soapbox.Identity.Authenticators.RemoveAuthenticator;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class RemoveAuthenticatorHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;

    public RemoveAuthenticatorHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result> RemoveAuthenticator()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
        if (user.IsFailure)
            return Error.NotFound("User not found.");

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "AuthenticatorKey");
        await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");
        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    } 
}
