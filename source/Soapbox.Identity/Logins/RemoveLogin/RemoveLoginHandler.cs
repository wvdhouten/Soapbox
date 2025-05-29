namespace Soapbox.Identity.Logins.RemoveLogin;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class RemoveLoginHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;

    public RemoveLoginHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result> RemoveLogin(string loginProvider, string providerKey)
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
        if (user.IsFailure)
            return Error.NotFound("User not found.");

        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.Count == 1 && !await _userManager.HasPasswordAsync(user))
            return Error.InvalidOperation("Cannot remove the last login method.");

        var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
            return Error.Unknown("The external login could not be removed.");

        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    } 
}
