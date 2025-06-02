namespace Soapbox.Identity.Profile.GetProfile;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using Soapbox.Identity.Profile;
using System.Security.Claims;

[Injectable]
public class GetProfileHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly ILogger<GetProfileHandler> _logger;

    public GetProfileHandler(TransactionalUserManager<SoapboxUser> userManager, SignInManager<SoapboxUser> signInManager, ILogger<GetProfileHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Result<UserProfile>> GetProfileAsync(ClaimsPrincipal claimPrincipal)
    {
        var userResult = await _userManager.GetUserAsync(claimPrincipal);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;

        var email = await _userManager.GetEmailAsync(user);
        var currentLogins = await _userManager.GetLoginsAsync(user);
        var otherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();

        return new UserProfile
        {
            Username = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = email ?? string.Empty,
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
            NewEmail = email ?? string.Empty,
            CurrentLogins = currentLogins,
            OtherLogins = otherLogins,
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
        };
    }
}
