namespace Soapbox.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Email;
    using Soapbox.Core.Settings;
    using Soapbox.Models;
    using Soapbox.Web.Models.Account;
    using Soapbox.Web.Services;

    [Authorize]
    public partial class AccountController : Controller
    {
        private readonly SiteSettings _siteSettings;
        private readonly AccountService _accountService;
        private readonly UserManager<SoapboxUser> _userManager;
        private readonly SignInManager<SoapboxUser> _signInManager;
        private readonly IEmailClient _emailClient;
        private readonly ILogger<AccountController> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(IOptionsSnapshot<SiteSettings> siteSettings,
            AccountService accountService,
            UserManager<SoapboxUser> userManager,
            SignInManager<SoapboxUser> signInManager,
            IEmailClient emailClient,
            ILogger<AccountController> logger,
            UrlEncoder urlEncoder)
        {
            _siteSettings = siteSettings.Value;
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailClient = emailClient;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            var currentLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();

            var model = new ProfileModel
            {
                Username = user.UserName,
                DisplayName = user.DisplayName,
                Email = email,
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
                NewEmail = email,
                CurrentLogins = currentLogins,
                OtherLogins = otherLogins,
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] ProfileModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (user.DisplayName != model.DisplayName)
            {
                user.DisplayName = model.DisplayName;
                await _userManager.UpdateAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);
            if (model.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(nameof(ConfirmEmailChange), "Account", values: new { userId, email = model.NewEmail, code }, protocol: Request.Scheme);
                await _emailClient.SendEmailAsync(model.NewEmail, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
            }

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var personalData = await _accountService.GetPersonalDataAsync(user);

            return File(JsonSerializer.SerializeToUtf8Bytes(personalData, new JsonSerializerOptions { WriteIndented = true }), "application/json", "PersonalData.json");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId is null || code is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            StatusMessage = "Your email has been confirmed.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
        {
            if (userId is null || email is null || code is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Updating your email failed. The email or code was incorrect.";

                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your email has been changed.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailVerification(SendEmailVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme);
            await _emailClient.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var model = new RegisterModel()
            {
                ReturnUrl = returnUrl ?? Url.Content("~/"),
                ExternalLogins = await GetExternalLogins()
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            model.ExternalLogins = await GetExternalLogins();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hasUsers = _userManager.Users.Any();
            var user = new SoapboxUser
            {
                UserName = model.Username,
                Email = model.Email,
                Role = !hasUsers ? UserRole.Administrator : UserRole.Subscriber
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code, returnUrl }, protocol: Request.Scheme);

                await _emailClient.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email, returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation([FromQuery] string email, [FromQuery] string returnUrl = null)
        {
            if (email is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new DeleteModel
            {
                RequirePassword = await _userManager.HasPasswordAsync(user)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] DeleteModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            model.RequirePassword = await _userManager.HasPasswordAsync(user);
            if (model.RequirePassword && !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return View(model);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation($"User with ID '{userId}' deleted themselves.");

            return Redirect("~/");
        }

        [HttpGet]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");
            }

            return View();
        }

        [HttpPost, ActionName("Disable2fa")]
        public async Task<IActionResult> Disable2fa_Post()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation($"User with ID '{_userManager.GetUserId(User)}' has disabled 2fa.");
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ForgetBrowser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' because they do not have 2FA enabled.");
            }

            var model = new RecoveryCodesModel
            {
                RecoveryCodes = (string[])TempData["RecoveryCodes"]
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RecoveryCodes(RecoveryCodesModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            model.RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation($"User with ID '{userId}' has generated new 2FA recovery codes.");

            StatusMessage = "You have generated new recovery codes.";

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ChangePasswordModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("User changed their password successfully.");
            }
            else
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            StatusMessage = "Your password has been set.";

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EnableAuthenticatorModel();
            await LoadSharedKeyAndQrCodeUriAsync(model, user);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAuthenticator(EnableAuthenticatorModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(model, user);
                return View(model);
            }

            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(model, user);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation($"User with ID '{userId}' has enabled 2FA with an authenticator app.");

            StatusMessage = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                TempData.Add("RecoveryCodes", recoveryCodes.ToArray());

                return RedirectToAction(nameof(RecoveryCodes));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(EnableAuthenticatorModel model, SoapboxUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            model.AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey[currentPosition..]);
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(AuthenticatorUriFormat, _urlEncoder.Encode("Soapbox"), _urlEncoder.Encode(email), unformattedKey);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return View();
        }

        [HttpPost, ActionName("RemoveAuthenticator")]
        public async Task<IActionResult> RemoveAuthenticator_Post()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);

            await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "AuthenticatorKey");
            await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");

            _logger.LogInformation($"User with ID '{user.Id}' has reset their authentication app key.");

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your authenticator app key has been reset.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddLogin(string provider)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = Url.Action(nameof(AddLoginCallback));
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> AddLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{user.Id}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info is null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToAction(nameof(Index));
            }

            await ClearExternalCookie();

            StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveLogin([FromForm] string loginProvider, [FromForm] string providerKey)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{user.Id}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not removed.";
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}