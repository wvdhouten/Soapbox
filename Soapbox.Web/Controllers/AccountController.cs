namespace Soapbox.Web.Controllers
{
    using System;
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
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.Identity;
    using Soapbox.Web.Config;
    using Soapbox.Web.Models.Account;

    [Authorize]
    public class AccountController : Controller
    {
        private readonly SiteSettings _siteSettings;
        private readonly UserManager<SoapboxUser> _userManager;
        private readonly SignInManager<SoapboxUser> _signInManager;
        private readonly IEmailClient _emailClient;
        private readonly ILogger<AccountController> _logger;

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(IOptionsSnapshot<SiteSettings> siteSettings,
            UserManager<SoapboxUser> userManager,
            SignInManager<SoapboxUser> signInManager,
            IEmailClient emailClient,
            ILogger<AccountController> logger)
        {
            _siteSettings = siteSettings.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailClient = emailClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(); // TODO: Default page
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var model = new LoginModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] LoginModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public IActionResult LoginExternal([FromForm] string provider, [FromQuery] string returnUrl = null)
        {
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

            // Sign in the user with this external login provider if the user already has a login.
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
            else
            {
                if (!_siteSettings.AllowRegistration)
                {
                    throw new InvalidOperationException("Registration is disabled");
                }

                // If the user does not have an account, then ask the user to create an account.
                var model = new LoginExternalModel
                {
                    ReturnUrl = returnUrl,
                    ProviderDisplayName = info.ProviderDisplayName
                };
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    model.Input = new LoginExternalModel.InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }

                return View(model);
            }
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginExternalConfirmation([FromBody] LoginExternalModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new SoapboxUser { UserName = model.Input.Email, Email = model.Input.Email };

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

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
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
            return View(nameof(LoginExternal), model);
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Login2fa([FromQuery] bool rememberMe, [FromQuery] string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            returnUrl ??= Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.Input.RememberMachine);

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
        public async Task<IActionResult> LoginRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
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
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout([FromQuery] string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            returnUrl ??= Url.Content("~/");

            return LocalRedirect(returnUrl);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Input.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { code }, Request.Scheme);

                await _emailClient.SendEmailAsync(model.Input.Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
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
                // Don't reveal that the user does not exist.
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

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId == null || code == null)
            {
                // TODO: right page.
                RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded
                ? "Thank you for confirming your email."
                : "Error confirming your email.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                // TODO: right page.
                return RedirectToAction(string.Empty);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Error changing email.";
                return View();
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Error changing user name.";
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you for confirming your email change.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return View(model);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme);
            await _emailClient.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var model = new RegisterModel()
            {
                ReturnUrl = returnUrl ?? Url.Content("~/"),
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterModel model, [FromQuery] string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new SoapboxUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Role = UserRole.Subscriber
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
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation([FromQuery] string email, [FromQuery] string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            var model = new RegisterConfirmationModel
            {
                Email = email,
                // Once you add a real email sender, you should remove this code that lets you confirm the account
                DisplayConfirmAccountLink = true
            };
            if (model.DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                model.EmailConfirmationUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code, returnUrl }, protocol: Request.Scheme);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
