namespace Soapbox.Identity.Authentication.RecoveryCodeLogin;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Authentication.Login;
using System.Threading.Tasks;

[Injectable]
public class RecoveryCodeLoginHandler
{
    private readonly SignInManager<SoapboxUser> _signInManager;

    public RecoveryCodeLoginHandler(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<Result> LoginAsync(RecoveryCodeLoginRequest request)
    {
        if (!await HasMfaRequestAsync())
            return Error.NotFound($"Unable to find MFA request.");

        var recoveryCode = request.RecoveryCode.Replace(" ", string.Empty);
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        return result switch
        {
            { Succeeded: true } => Result.Success(result),
            { IsLockedOut: true } => Error.Other(LoginRequest.LockedOut),
            _ => Error.ValidationError("Login failed.", new() { { string.Empty, "Invalid recovery code." } })
        };
    }

    public async Task<bool> HasMfaRequestAsync()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        return user is not null;
    }
}
