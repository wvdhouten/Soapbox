namespace Soapbox.Web.Account.Profile.GetProfile;

using System.Security.Claims;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Identity.Managers;
using Soapbox.Web.Models.Account;

[Injectable]
public class GetProfileQuery
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly ILogger<GetProfileQuery> _logger;

    public GetProfileQuery(TransactionalUserManager<SoapboxUser> userManager, SignInManager<SoapboxUser> signInManager, ILogger<GetProfileQuery> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Result<ProfileModel>> HandleAsync(ClaimsPrincipal claimPrincipal)
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

        return new ProfileModel
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
