namespace Soapbox.Identity.Authentication.ProcessExternalLogin;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Soapbox.Application.Settings;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Authentication.Login;

[Injectable]
public class ProcessExternalLoginHandler
{
    private readonly SiteSettings _siteSettings;
    private readonly SignInManager<SoapboxUser> _signInManager;

    public ProcessExternalLoginHandler(IOptionsSnapshot<SiteSettings> siteSettings, SignInManager<SoapboxUser> signInManager)
    {
        _siteSettings = siteSettings.Value;
        _signInManager = signInManager;
    }

    public async Task<Result> ProcessExternalLogin()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Error.NotFound("Error loading external login information.");

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        return result switch
        {
            { Succeeded: true } => Result.Success(),
            { RequiresTwoFactor: true } => Error.Other(LoginRequest.RequiresTwoFactor, "Two-factor authentication is required."),
            { IsLockedOut: true } => Error.Other(LoginRequest.LockedOut, "User account is locked out."),
            { IsNotAllowed: true } => Error.InvalidOperation("User account is not allowed to log in."),
            { } when !_siteSettings.AllowRegistration => Error.InvalidOperation("Registration is disabled."),
            _ => Error.Other("RequiresRegistration")
        };
    }
}
