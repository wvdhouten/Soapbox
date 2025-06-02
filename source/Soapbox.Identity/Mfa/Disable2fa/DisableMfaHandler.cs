namespace Soapbox.Identity.Mfa.Disable2fa;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class DisableMfaHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly ILogger<DisableMfaHandler> _logger;

    public DisableMfaHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        ILogger<DisableMfaHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<bool>> DisableMfaAsync()
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(userResult.Value, false);
        if (!disable2faResult.Succeeded)
            return Error.InvalidOperation($"Unable to disable 2FA for the current user.");

        _logger.LogInformation("User '{UserId}' has disabled 2fa.", userResult.Value.Id);

        return true;
    }
}
