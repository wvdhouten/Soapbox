namespace Soapbox.Web.Account.Logins.GetExternalLogins;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;

[Injectable]
public class GetExternalLoginsQuery
{
    private readonly SignInManager<SoapboxUser> _signInManager;
    public GetExternalLoginsQuery(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<Result<IEnumerable<AuthenticationScheme>>> HandleAsync() => 
        Result.Success(await _signInManager.GetExternalAuthenticationSchemesAsync());
}
