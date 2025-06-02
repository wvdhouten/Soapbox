namespace Soapbox.Identity.Authentication.Logout;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;
using System.Threading.Tasks;

[Injectable]
public class LogoutHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;

    public LogoutHandler(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task LogoutAsync() => await _signInManager.SignOutAsync();
}
