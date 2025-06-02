namespace Soapbox.Identity.Logins.GetExternalLogins;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;

[Injectable]
public class GetExternalLoginsHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetExternalLoginsHandler(
        SignInManager<SoapboxUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task ClearExternalCookieAsync() => await _httpContextAccessor.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

    public async Task<Result<IEnumerable<AuthenticationScheme>>> GetExternalLoginsAsync() => Result.Success(await _signInManager.GetExternalAuthenticationSchemesAsync());
}
