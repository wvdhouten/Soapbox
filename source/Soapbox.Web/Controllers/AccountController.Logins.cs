namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Logins.RemoveLogin;

public partial class AccountController
{
    [HttpPost]
    public async Task<IActionResult> AddLogin([FromServices] AddLoginHandler handler, string provider)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var result = await handler.GetLoginAuthenticationProperties(provider);
        return result switch
        {
            { IsSuccess: true } => Challenge(result, provider),
            _ => throw new InvalidOperationException("An unexpected error occurred.")
        };
    }

    [HttpGet]
    public async Task<IActionResult> AddLoginConfirmation([FromServices] AddLoginHandler handler)
    {
        var result = await handler.AddLogin();
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("The external login was added.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest("User not found."),
            { IsFailure: true, Error.Code: ErrorCode.InvalidOperation } => WithStatusMessage(result.Error.Message).RedirectToAction(nameof(Index)),
            _ => throw new InvalidOperationException("An unexpected error occurred.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> RemoveLogin([FromServices] RemoveLoginHandler handler, [FromForm] string loginProvider, [FromForm] string providerKey)
    {
        var result = await handler.RemoveLogin(loginProvider, providerKey);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("The external login was removed.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest("User not found."),
            { IsFailure: true, Error.Code: ErrorCode.InvalidOperation } => WithStatusMessage(result.Error.Message).RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.Unknown } => WithStatusMessage(result.Error.Message).RedirectToAction(nameof(Index)),
            _ => throw new InvalidOperationException("An unexpected error occurred.")
        };
    }
}
