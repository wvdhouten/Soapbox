namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Email.ConfirmEmail;
using Soapbox.Identity.Email.SendEmailVerification;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromServices] ConfirmEmailHandler handler, [FromQuery] string userId, [FromQuery] string code)
    {
        if (userId is null || code is null)
            return RedirectToAction(nameof(Index));

        var result = await handler.ConfirmEmail(userId, code);
        StatusMessage = result.IsSuccess
            ? "Your email has been confirmed."
            : "Something went wrong.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange([FromServices] ConfirmEmailChangeHandler handler, [FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
    {
        if (userId is null || email is null || code is null)
            return RedirectToAction(nameof(Index));

        var result = await handler.ConfirmEmailChange(userId, email, code);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Your email has been changed.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest("User not found."),
            { IsFailure: true } => WithStatusMessage("Updating your email failed. The email or code was incorrect.").RedirectToAction(nameof(Index)),
            _ => throw new InvalidOperationException("Something went wrong.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> SendEmailVerification([FromServices] SendEmailVerificationHandler handler, [FromForm] SendEmailVerificationRequest request)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var result = await handler.SendEmailVerificationEmail(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Verification email sent. Please check your email.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest("User not found."),
            _ => throw new InvalidOperationException()
        };
    }
}
