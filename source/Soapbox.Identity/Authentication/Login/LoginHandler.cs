namespace Soapbox.Identity.Authentication.Login;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Logins.GetExternalLogins;

[Injectable]
public class LoginHandler : GetExternalLoginsHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;

    public LoginHandler(
        SignInManager<SoapboxUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
        : base(signInManager, httpContextAccessor)
    {
        _signInManager = signInManager;
    }

    public async Task<Result> LoginAsync(LoginRequest request)
    {
        var triggerLockoutOnFailure = false;
        var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, request.RememberMe, triggerLockoutOnFailure);
        return result switch
        {
            { Succeeded: true } => Result.Success(),
            { RequiresTwoFactor: true } => Error.Other(LoginRequest.RequiresTwoFactor, "Two-factor authentication is required."),
            { IsLockedOut: true } => Error.Other(LoginRequest.LockedOut, "User account is locked out."),
            _ => Result.Failure(Error.ValidationError("Invalid login attempt.", new() { { string.Empty, "Invalid login attempt." } }))
        };
    }
}
