namespace Soapbox.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Identity;
    using Soapbox.Web.Models.Account;

    public partial class AccountController : Controller
    {
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }

            await ClearExternalCookie();

            var model = new LoginModel
            {
                ExternalLogins = await GetExternalLogins(),
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var triggerLockoutOnFailure = false;
                var result = await _signInManager.PasswordSignInAsync(model.Input.Username, model.Input.Password, model.Input.RememberMe, triggerLockoutOnFailure);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(Login2fa), new { ReturnUrl = returnUrl, model.Input.RememberMe });
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

            model.ExternalLogins = await GetExternalLogins();
            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public IActionResult LoginExternal([FromForm] string provider, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Action(nameof(LoginExternalCallback), new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> LoginExternalCallback([FromQuery] string returnUrl = null, [FromQuery] string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation($"{info.Principal.Identity.Name} logged in with {info.LoginProvider} provider.");
                return LocalRedirect(returnUrl);
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
                ProviderDisplayName = info.ProviderDisplayName,
                Input = new LoginExternalModel.InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty
                }
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginExternalConfirmation([FromForm] LoginExternalModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new SoapboxUser { UserName = model.Input.Username, Email = model.Input.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User created an account using {info.LoginProvider} provider.");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme);

                        await _emailClient.SendEmailAsync(model.Input.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToAction(nameof(RegisterConfirmation), new { model.Input.Email });
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

            model.ProviderDisplayName = info.ProviderDisplayName;
            model.ReturnUrl = returnUrl;
            return View(nameof(LoginExternalCallback), model);
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Login2fa([FromQuery] bool rememberMe, [FromQuery] string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var model = new Login2faModel
            {
                ReturnUrl = returnUrl,
                RememberMe = rememberMe
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login2fa([FromForm] Login2faModel model, [FromQuery] bool rememberMe, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.Input.AuthenticatorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User with ID '{user.Id}' logged in with 2fa.");
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning($"User with ID '{user.Id}' account locked out.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning($"Invalid authenticator code entered for user with ID '{user.Id}'.");
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View(model);
            }
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> LoginRecoveryCode([FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginRecoveryCodeModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginRecoveryCode([FromForm] LoginRecoveryCodeModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.Input.RecoveryCode.Replace(" ", string.Empty);
            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User with ID '{user.Id}' logged in with a recovery code.");
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning($"User with ID '{user.Id}' account locked out.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning($"Invalid recovery code entered for user with ID '{user.Id}'");
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View(model);
            }
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Input.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var code = await _accountService.GeneratePasswordResetCode(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { code }, Request.Scheme);
            await _emailClient.SendEmailAsync(model.Input.Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            // TODO: Consider status message and redirect.
            return View();
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword([FromQuery]string code = null)
        {
            var model = new ResetPasswordModel();
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
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

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            // TODO: Consider status message and redirect.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout([FromQuery] string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            returnUrl ??= Url.Content("~/");

            return LocalRedirect(returnUrl);
        }

        private async Task ClearExternalCookie()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        private async Task<IList<AuthenticationScheme>> GetExternalLogins()
        {
            return (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }
    }
}
