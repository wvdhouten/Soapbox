namespace Soapbox.Identity.Authentication.MfaLogin;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Authentication.Login;
using System.Threading.Tasks;

[Injectable]
public class MfaLoginHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;

    public MfaLoginHandler(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<Result> LoginAsync(MfaLoginRequest request)
    {
        if (!await HasMfaRequestAsync())
            return Error.NotFound($"Unable to find MFA request.");

        var authenticatorCode = request.AuthenticatorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, request.RememberMe, request.RememberMachine);

        return result switch
        {
            { Succeeded: true } => Result.Success(result),
            { IsLockedOut: true } => Error.Other(LoginRequest.LockedOut),
            _ => Error.ValidationError("Login failed.", new() { { string.Empty, "Invalid authenticator code." } })
        };
    }

    public async Task<bool> HasMfaRequestAsync()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        return user is not null;
    }
}
