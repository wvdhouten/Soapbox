namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Logins.GetExternalLogins;
using Soapbox.Identity.Registration.ConfirmRegistration;
using Soapbox.Identity.Registration.RegisterAccount;

public partial class AccountController
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromServices] GetExternalLoginsHandler query,
        [FromQuery] string? returnUrl = null)
    {
        var externalLoginsResult = await query.GetExternalLoginsAsync();
        var model = new RegisterAccountRequest()
        {
            ReturnUrl = returnUrl ?? Url.Content("~/"),
            ExternalLogins = [.. externalLoginsResult.Value]
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromServices] RegisterAccountHandler command,
        [FromServices] GetExternalLoginsHandler query,
        [FromForm] RegisterAccountRequest model,
        [FromQuery] string? returnUrl = null)
    {
        var externalLoginsResult = await query.GetExternalLoginsAsync();
        model.ExternalLogins = [.. externalLoginsResult.Value];

        if (!ModelState.IsValid)
            return View(model);

        var registrationResult = await command.HandleAsync(model, returnUrl ?? Url.Content("~/"));
        return registrationResult.IsSuccess switch
        {
            false when registrationResult.Error is ValidationError validationError
                => ValidationError(nameof(Register), model, validationError.Errors),
            true when registrationResult.Value is true
                => WithStatusMessage("Your account has been created. Please check your email for confirmation.").RedirectToAction(nameof(ConfirmRegistration)),
            true when registrationResult.Value is false
                => WithStatusMessage("Your account has been created.").LocalRedirect(returnUrl ??= Url.Content("~/")),
            _ => BadRequest("Unable to process request."),
        };
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmRegistration([FromServices] UserExistsHandler handler, [FromQuery] string email)
    {
        // TODO: What is this page used for? Can it be merged?
        if (email is null)
            return RedirectToAction(nameof(Index));

        var result = await handler.UserWithEmailExistsAsync(email);
        return result switch
        {
            { IsSuccess: true } => View(),
            { IsFailure: true } => NotFound($"Unable to load user with email '{email}'."),
            _ => throw new InvalidOperationException()
        };
    }
}
