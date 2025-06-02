namespace Soapbox.Identity.Mfa.GenerateRecoveryCodes;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class GenerateRecoveryCodesHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodesHandler> _logger;

    public GenerateRecoveryCodesHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        ILogger<GenerateRecoveryCodesHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<string[]>> GenerateRecoveryCodesAsync()
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(userResult.Value);
        if (!isTwoFactorEnabled)
            return Error.InvalidOperation($"Cannot generate recovery codes. 2FA is disabled.");

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(userResult.Value, 10);
        if (recoveryCodes is null)
            return Error.Unknown();

        _logger.LogInformation("User '{UserId}' has generated new 2FA recovery codes.", userResult.Value.Id);
        return recoveryCodes.ToArray();
    }
}
