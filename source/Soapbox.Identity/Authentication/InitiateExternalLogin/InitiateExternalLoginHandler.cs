namespace Soapbox.Identity.Authentication.InitiateExternalLogin;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;

[Injectable]
public class InitiateExternalLoginHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;

    public InitiateExternalLoginHandler(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public Result<AuthenticationProperties> InitiateExternalLogin(string provider, string redirectUrl)
        => _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
}
