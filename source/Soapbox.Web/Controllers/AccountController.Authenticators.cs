namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Authenticators.AddAuthenticator;
using Soapbox.Identity.Authenticators.GetAuthenticatorEnabled;
using Soapbox.Identity.Authenticators.RemoveAuthenticator;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> AddAuthenticator([FromServices] AddAuthenticatorHandler handler)
    {
        var result = await handler.CreateAddAuthenticatorRequestAsync();
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(result.Error.Message),
            _ => BadRequest()
        };
    }

    [HttpPost]
    public async Task<IActionResult> AddAuthenticator([FromServices] AddAuthenticatorHandler handler, AddAuthenticatorRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestResult = await handler.CreateAddAuthenticatorRequestAsync(request);
            return requestResult switch
            {
                { IsSuccess: true } => View(requestResult.Value),
                { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(requestResult.Error.Message),
                _ => throw new InvalidOperationException("An unexpected error occurred while creating the authenticator request.")
            };
        }

        var result = await handler.AddAuthenticatorAsync(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Your authenticator app has been verified.").View("RecoveryCodes", result.Value),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(result.Error.Message),
            { IsFailure: true } when result.Error is ValidationError<AddAuthenticatorRequest> error => ValidationError(nameof(AddAuthenticator), error.Request, error.Errors),
            _ => throw new InvalidOperationException("An unexpected error occurred while creating the authenticator request.")
        };
    }

    [HttpGet]
    public async Task<IActionResult> RemoveAuthenticator([FromServices] HasAuthenticatorEnabledHandler handler)
    {
        var result = await handler.HasAuthenticatorEnabled();
        return result switch
        {
            { IsSuccess: true, Value: true } when result.Value => View(),
            { IsSuccess: true, Value: false } when !result.Value => BadRequest("Authenticator is not enabled."),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            _ => BadRequest("An unexpected error occurred."),
        };
    }

    [HttpPost]
    public async Task<IActionResult> RemoveAuthenticator([FromServices] RemoveAuthenticatorHandler handler)
    {
        var result = await handler.RemoveAuthenticator();
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(result.Error.Message),
            _ => throw new InvalidOperationException()
        };
    }
}
