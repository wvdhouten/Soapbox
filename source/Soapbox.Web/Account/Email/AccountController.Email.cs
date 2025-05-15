namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Web.Account.Email.ConfirmEmail;
using Soapbox.Web.Account.Shared;
using Soapbox.Web.Models.Account;
using System.Text;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromServices] ConfirmEmailCommand command, [FromQuery] string userId, [FromQuery] string code)
    {
        if (userId is null || code is null)
            return RedirectToAction(nameof(Index));

        var result = await command.HandleAsync(userId, code);

        StatusMessage = result.IsSuccess
            ? "Your email has been confirmed."
            : "Something went wrong.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
    {
        if (userId is null || email is null || code is null)
            return RedirectToAction(nameof(Index));

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound($"Unable to load user with ID '{userId}'.");

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
            return RedirectToAction(nameof(Index));

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            StatusMessage = "User not found.";

            return RedirectToAction(nameof(Index));
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme)!;
        await _emailService.SendEmailAsync(model.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });

        StatusMessage = "Verification email sent. Please check your email.";

        return RedirectToAction(nameof(Index));
    }
}
