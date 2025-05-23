namespace Soapbox.Web.Account;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Soapbox.Application;
using Soapbox.Domain.Users;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Models.Account;
using Soapbox.Web.Account.Shared;
using Soapbox.Web.Account.Logins.GetExternalLogins;

public partial class AccountController : SoapboxControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login([FromServices] GetExternalLoginsQuery query, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return LocalRedirect(returnUrl);
        }

        await ClearExternalCookie();

        var externalLoginsResult = await query.HandleAsync();
        var model = new LoginModel
        {
            ExternalLogins = [.. externalLoginsResult.Value],
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromServices] GetExternalLoginsQuery query, [FromForm] LoginModel model, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (ModelState.IsValid)
        {
            var triggerLockoutOnFailure = false;
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, triggerLockoutOnFailure);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(Login2fa), new { ReturnUrl = returnUrl, model.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        var externalLoginsResult = await query.HandleAsync();
        model.ExternalLogins = [.. externalLoginsResult.Value];
        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public IActionResult LoginExternal([FromForm] string provider, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var redirectUrl = Url.Action(nameof(LoginExternalCallback), new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> LoginExternalCallback([FromQuery] string? returnUrl = null, [FromQuery] string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("{User} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(Login2fa), new { ReturnUrl = returnUrl });
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction(nameof(Lockout));
        }

        if (!_siteSettings.AllowRegistration)
        {
            throw new InvalidOperationException("Registration is disabled");
        }

        var model = new LoginExternalModel
        {
            ReturnUrl = returnUrl,
            ProviderDisplayName = info.ProviderDisplayName ?? "Unknown",
            Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty
        };

        ViewData[Constants.PageTitle] = $"Sign in: {info.ProviderDisplayName}";

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> LoginExternalConfirmation([FromForm] LoginExternalModel model, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var hasUsers = _userManager.Users.Any();
            var user = new SoapboxUser
            {
                UserName = model.Username,
                Email = model.Email,
                Role = !hasUsers ? UserRole.Administrator : UserRole.Subscriber
            };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Provider} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme);

                    await _emailService.SendEmailAsync(model.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl! });

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction(nameof(ConfirmRegistration), new { model.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        model.ProviderDisplayName = info.ProviderDisplayName ?? "Unknown";
        model.ReturnUrl = returnUrl;
        return View(nameof(LoginExternalCallback), model);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login2fa([FromQuery] bool rememberMe, [FromQuery] string? returnUrl = null)
    {
        _ = await _signInManager.GetTwoFactorAuthenticationUserAsync()
            ?? throw new InvalidOperationException("Unable to load two-factor authentication user.");

        var model = new Login2faModel
        {
            ReturnUrl = returnUrl,
            RememberMe = rememberMe
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login2fa([FromForm] Login2faModel model, [FromQuery] bool rememberMe, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var authenticatorCode = model.AuthenticatorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View(model);
        }
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> LoginRecoveryCode([FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        _ = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var model = new LoginRecoveryCodeModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> LoginRecoveryCode([FromForm] LoginRecoveryCodeModel model, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}'", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return View(model);
        }
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Lockout() => View();

    [HttpGet, AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        StatusMessage = "Please check your email to reset your password.";

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        var code = await _accountService.GeneratePasswordResetCode(user);
        var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { code }, Request.Scheme)!;
        await _emailService.SendEmailAsync(model.Email, "Reset Password", new ResetPassword { CallbackUrl = callbackUrl });

        return RedirectToAction(nameof(Index));
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ResetPassword([FromQuery] string? code = null)
    {
        var model = new ResetPasswordModel();
        if (code is null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            model.Code = code;
            return View(model);
        }
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        StatusMessage = "Your password has been reset.";

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
        var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out.");

        return LocalRedirect(returnUrl);
    }

    private async Task ClearExternalCookie() 
        => await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
}
