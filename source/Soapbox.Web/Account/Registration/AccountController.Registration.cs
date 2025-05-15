namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Users;
using Soapbox.Web.Models.Account;
using Soapbox.Web.Account.Shared;
using System.Text;
using Soapbox.Web.Account.Logins.GetExternalLogins;

public partial class AccountController
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromServices] GetExternalLoginsQuery query,
        [FromQuery] string? returnUrl = null)
    {
        var externalLoginsResult = await query.HandleAsync();
        var model = new RegisterModel()
        {
            ReturnUrl = returnUrl ?? Url.Content("~/"),
            ExternalLogins = [.. externalLoginsResult.Value]
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Register([FromServices] GetExternalLoginsQuery query, [FromForm] RegisterModel model, [FromQuery] string? returnUrl = null)
    {
        var externalLoginsResult = await query.HandleAsync();
        model.ExternalLogins = [.. externalLoginsResult.Value];

        if (!ModelState.IsValid)
            return View(model);

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
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code, returnUrl }, protocol: Request.Scheme)!;

            try
            {
                await _emailService.SendEmailAsync(model.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });
            }
            catch
            {
                // TODO: What if the email is not sent?
            }

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                return RedirectToAction(nameof(ConfirmRegistration), new { email = model.Email });
            else
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ??= Url.Content("~/"));
            }
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmRegistration([FromQuery] string email)
    {
        // TODO: What is this page used for? Can it be merged?
        if (email is null)
            return RedirectToAction(nameof(Index));

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        return View();
    }
}
