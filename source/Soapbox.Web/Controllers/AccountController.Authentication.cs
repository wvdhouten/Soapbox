namespace Soapbox.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Controllers.Base;
using Soapbox.Domain.Results;
using Soapbox.Identity.Authentication.Login;
using Soapbox.Identity.Authentication.Logout;
using Soapbox.Identity.Authentication.MfaLogin;
using Soapbox.Identity.Authentication.RecoveryCodeLogin;
using Soapbox.Identity.Authentication.ExternalLoginRegistration;
using Soapbox.Identity.Authentication.InitiateExternalLogin;
using Soapbox.Identity.Authentication.ProcessExternalLogin;
using Soapbox.Application.Constants;
using Soapbox.Identity.Password.ForgotPassword;
using Soapbox.Identity.Password.ResetPassword;

public partial class AccountController
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromServices] LoginHandler handler,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (User.Identity?.IsAuthenticated ?? false)
            return LocalRedirect(returnUrl);

        await handler.ClearExternalCookieAsync();

        var externalLoginsResult = await handler.GetExternalLoginsAsync();
        return View(new LoginRequest
        {
            ExternalLogins = [.. externalLoginsResult.Value],
            ReturnUrl = returnUrl
        });
    }

    [HttpPost, AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromServices] LoginHandler handler,
        [FromForm] LoginRequest request,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
            return WithStatusMessage("Invalid login attempt. Please try again.")
                .View(request.WithExternalLogins((await handler.GetExternalLoginsAsync()).Value));

        var result = await handler.LoginAsync(request);
        return result switch
        {
            { IsSuccess: true } => LocalRedirect(returnUrl),
            { IsFailure: true } when result.Error is ValidationError error => ValidationError(nameof(Login), request, error.Errors),
            { IsFailure: true, Error.Code: LoginRequest.RequiresTwoFactor } => RedirectToAction(nameof(MfaLogin), new { ReturnUrl = returnUrl, request.RememberMe }),
            { IsFailure: true, Error.Code: LoginRequest.LockedOut } => RedirectToAction(nameof(Lockout)),
            _ => WithStatusMessage("Invalid login attempt. Please try again.")
                .View(request.WithExternalLogins((await handler.GetExternalLoginsAsync()).Value))
        };
    }

    [HttpPost, AllowAnonymous]
    public IActionResult ExternalLogin(
        [FromServices] InitiateExternalLoginHandler handler,
        [FromForm] string provider,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var redirectUrl = Url.Action(nameof(ExternalLoginConfirmation), new { returnUrl });
        var result = handler.InitiateExternalLogin(provider, redirectUrl!);

        return Challenge(result, provider);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> ExternalLoginConfirmation(
        [FromServices] ProcessExternalLoginHandler handler,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError is not null)
            return WithErrorMessage($"Error from external provider: {remoteError}").RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });

        var result = await handler.ProcessExternalLogin();
        return result switch
        {
            { IsSuccess: true } => LocalRedirect(returnUrl),
            { IsFailure: true, Error.Code: ErrorCode.NotFound }
                => WithErrorMessage("Error loading external login information.").RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl }),
            { IsFailure: true, Error.Code: LoginRequest.RequiresTwoFactor }
                => RedirectToAction(nameof(MfaLogin), new { ReturnUrl = returnUrl }),
            { IsFailure: true, Error.Code: LoginRequest.LockedOut }
                => RedirectToAction(nameof(Lockout)),
            { IsFailure: true, Error.Code: ErrorCode.InvalidOperation }
                => WithErrorMessage(result.Error.Message).RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl }),
            { IsFailure: true, Error.Code: ProcessExternalLoginHandler.RequiresRegistration } when result.Error is ValueError<ExternalLoginRegistrationRequest> error
                => WithViewData(ViewConstants.PageTitle, $"Sign in: {error.Value.ProviderDisplayName}").View(error.Value),
            _ => BadRequest(),
        };
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> ExternalLoginConfirmation([FromServices] ExternalLoginRegistrationHandler handler, [FromForm] ExternalLoginRegistrationRequest request)
    {
        if (!ModelState.IsValid)
            return View(nameof(ExternalLoginConfirmation), request);

        var result = await handler.RegisterExternalLogin(request);
        return result switch
        {
            { IsSuccess: true } when result is ExternalLoginRegistrationResult successResult => successResult.RequiresConfirmation
                ? RedirectToAction(nameof(ConfirmRegistration), new { request.Email })
                : LocalRedirect(request.ReturnUrl ?? Url.Content("~/")),
            { IsFailure: true, Error.Code: ErrorCode.NotFound }
                => WithErrorMessage("Error loading external login information during confirmation.").RedirectToAction(nameof(Login), new { ReturnUrl = request.ReturnUrl ?? Url.Content("~/") }),
            { IsFailure: true } when result.Error is ValidationError error
                => WithErrorMessage(error.Message).ValidationError(nameof(ExternalLoginConfirmation), request, error.Errors),
            _ => BadRequest()
        };
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> MfaLogin([FromServices] MfaLoginHandler handler, [FromQuery] bool rememberMe, [FromQuery] string? returnUrl = null)
    {
        return !await handler.HasMfaRequestAsync()
            ? BadRequest()
            : View(new MfaLoginRequest { ReturnUrl = returnUrl ?? Url.Content("~/"), RememberMe = rememberMe });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> MfaLogin([FromServices] MfaLoginHandler handler, [FromForm] MfaLoginRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.LoginAsync(request);
        return result switch
        {
            { IsSuccess: true } => LocalRedirect(request.ReturnUrl ?? Url.Content("~/")),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(result.Error.Message),
            { IsFailure: true, Error.Code: LoginRequest.LockedOut } => RedirectToAction(nameof(Lockout)),
            { IsFailure: true } when result.Error is ValidationError error => ValidationError(nameof(MfaLogin), request, error.Errors),
            _ => BadRequest()
        };
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> RecoveryCodeLogin([FromServices] RecoveryCodeLoginHandler handler, [FromQuery] string? returnUrl = null)
    {
        return !await handler.HasMfaRequestAsync()
            ? BadRequest()
            : View(new RecoveryCodeLoginRequest { ReturnUrl = returnUrl ?? Url.Content("~/") });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> RecoveryCodeLogin([FromServices] RecoveryCodeLoginHandler handler, [FromForm] RecoveryCodeLoginRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.LoginAsync(request);
        return result switch
        {
            { IsSuccess: true } => LocalRedirect(request.ReturnUrl ?? Url.Content("~/")),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => BadRequest(result.Error.Message),
            { IsFailure: true, Error.Code: LoginRequest.LockedOut } => RedirectToAction(nameof(Lockout)),
            { IsFailure: true } when result.Error is ValidationError error => ValidationError(nameof(MfaLogin), request, error.Errors),
            _ => BadRequest()
        };
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Lockout() => View();

    [HttpGet, AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromServices] ForgotPasswordHandler handler, [FromForm] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.ForgotPasswordAsync(request);
        return result switch
        {
            { IsSuccess: true } or { IsFailure: true, Error.Code: ErrorCode.NotFound }
                => WithStatusMessage("Please check your email to reset your password.").RedirectToAction(nameof(Index)),
            { IsFailure: true }
                => BadRequest(result.Error?.Message),
            _ => BadRequest()
        };
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ResetPassword([FromQuery] string? code = null)
    {
        return code is null
            ? BadRequest("A code must be supplied for password reset.")
            : View(new ResetPasswordRequest() { Code = code });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromServices] ResetPasswordHandler handler, [FromForm] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.ResetPasswordAsync(request);
        return result switch
        {
            { IsSuccess: true } or { IsFailure: true, Error.Code: ErrorCode.NotFound }
                => WithStatusMessage("Your password has been reset.").RedirectToAction(nameof(Index)),
            { IsFailure: true } when result.Error is ValidationError validationError
                => ValidationError(nameof(ResetPassword), request, validationError.Errors),
            { IsFailure: true }
                => BadRequest(result.Error?.Message),
            _ => BadRequest()
        };
    }

    [HttpPost]
    public async Task<IActionResult> Logout([FromServices] LogoutHandler handler, [FromQuery] string? returnUrl = null)
    {
        await handler.LogoutAsync();

        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }
}
