namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Password.ChangePassword;
using Soapbox.Identity.Password.GetPasswordStatus;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> ChangePassword(
        [FromServices] GetPasswordStatusHandler query)
    {
        var result = await query.GetPasswordStatusAsync();
        return result switch
        {
            { IsSuccess: true } => View(new ChangePasswordRequest { HasPassword = result.Value.HasPassword }),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            _ => BadRequest("Unable to process request.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(
        [FromServices] ChangePasswordHandler command,
        [FromForm] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await command.ChangePasswordAsync(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Your password has been changed.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            { IsFailure: true } when result.Error is ValidationError validationError => ValidationError(nameof(ChangePassword), request, validationError.Errors),
            _ => BadRequest("Unable to process request.")
        };
    }
}
