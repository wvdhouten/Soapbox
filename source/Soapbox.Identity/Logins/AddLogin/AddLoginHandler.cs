namespace Soapbox.Identity.Logins.RemoveLogin;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Logins.GetExternalLogins;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Shared;

[Injectable]
public class AddLoginHandler : GetExternalLoginsHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IUrlHelper _urlHelper;

    public AddLoginHandler(
        IHttpContextAccessor httpContextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IUrlHelper urlHelper)
        : base(signInManager, httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _urlHelper = urlHelper;
    }

    public async Task<Result<AuthenticationProperties>> GetLoginAuthenticationProperties(string provider)
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var redirectUrl = _urlHelper.Action("Account", "AddLoginConfirmation");

        return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(_httpContextAccessor.HttpContext.User));
    }

    public async Task<Result> AddLogin()
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user.IsFailure)
            return Error.NotFound("User not found.");

        var info = await _signInManager.GetExternalLoginInfoAsync(user.Value.Id);
        var result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
            return Error.InvalidOperation("The external login could not be added. External logins can only be associated with one account.");

        await ClearExternalCookieAsync();

        return Result.Success();
    }
}
