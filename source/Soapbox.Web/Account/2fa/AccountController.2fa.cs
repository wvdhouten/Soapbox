namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Models.Account;

public partial class AccountController : SoapboxControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Disable2fa()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to find user with ID '{_userManager.GetUserId(User)}'.");

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");

        return View();
    }

    [HttpPost, ActionName("Disable2fa")]
    public async Task<IActionResult> Disable2fa_Post()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
        StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ForgetBrowser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
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
            RecoveryCodes = (string[])TempData["RecoveryCodes"]!
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

        // TODO: ErrorHandling
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        if (recoveryCodes is not null)
            model.RecoveryCodes = [.. recoveryCodes];

        _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);

        StatusMessage = "You have generated new recovery codes.";

        return View(model);
    }
}
