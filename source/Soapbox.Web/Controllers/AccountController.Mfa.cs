namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Mfa.Disable2fa;
using Soapbox.Identity.Mfa.ForgetClient;
using Soapbox.Identity.Mfa.GenerateRecoveryCodes;
using Soapbox.Identity.Mfa.Get2faEnabled;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> Disable2fa([FromServices] GetMfaEnabledHandler query)
    {
        var result = await query.GetMfaEnabledAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            true when !result.Value => BadRequest("2FA is already disabled for the current user."),
            true when result.Value => View(),
            _ => BadRequest("An unexpected error occurred."),
        };
    }

    [HttpPost, ActionName("Disable2fa")]
    public async Task<IActionResult> Disable2fa_Post([FromServices] DisableMfaHandler command)
    {
        var result = await command.DisableMfaAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            true when !result.Value => BadRequest("2FA is already disabled for the current user."),
            true when result.Value => WithStatusMessage("2fa has been disabled. Setup an authenticator app to re-enable it.").RedirectToAction(nameof(Index)),
            _ => BadRequest("An unexpected error occurred."),
        };
    }

    [HttpPost]
    public async Task<IActionResult> ForgetBrowser([FromServices] ForgetClientHandler command)
    {
        var result = await command.ForgetClientAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            true when result.Value => WithStatusMessage("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.").RedirectToAction(nameof(Index)),
            _ => BadRequest("An unexpected error occurred."),
        };
    }

    [HttpPost]
    public async Task<IActionResult> RecoveryCodes([FromServices] GenerateRecoveryCodesHandler command)
    {
        var result = await command.GenerateRecoveryCodesAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false when result.Error?.Code == ErrorCode.InvalidOperation => BadRequest(result.Error.Message),
            true => WithStatusMessage("You have generated new recovery codes.").View(result.Value),
            _ => BadRequest("An unexpected error occurred."),
        };
    }
}
