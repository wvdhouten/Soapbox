namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Users;
using Soapbox.Web.Models.Account;
using System.Text;

public partial class AccountController
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    [HttpGet]
    public async Task<IActionResult> AddAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var model = new EnableAuthenticatorModel();
        await LoadSharedKeyAndQrCodeUriAsync(model, user);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddAuthenticator(EnableAuthenticatorModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

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
        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

        StatusMessage = "Your authenticator app has been verified.";

        if (await _userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            TempData.Add("RecoveryCodes", recoveryCodes.ToArray());

            return RedirectToAction(nameof(RecoveryCodes));
        }
        else
            return RedirectToAction(nameof(Index));
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(EnableAuthenticatorModel model, SoapboxUser user)
    {
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrEmpty(unformattedKey))
            throw new InvalidOperationException($"Unable to load authenticator key for user with ID '{user.Id}'.");

        model.SharedKey = FormatKey(unformattedKey);
        model.AuthenticatorUri = GenerateQrCodeUri(user.Id, unformattedKey);
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
            result.Append(unformattedKey[currentPosition..]);

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
        => string.Format(AuthenticatorUriFormat,
            _urlEncoder.Encode("Soapbox"),
            _urlEncoder.Encode(email),
            unformattedKey);

    [HttpGet]
    public async Task<IActionResult> RemoveAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        return View();
    }

    [HttpPost, ActionName("RemoveAuthenticator")]
    public async Task<IActionResult> RemoveAuthenticator_Post()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user.IsFailure)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        await _userManager.SetTwoFactorEnabledAsync(user, false);

        await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "AuthenticatorKey");
        await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");

        _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Value.Id);

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your authenticator app key has been reset.";

        return RedirectToAction(nameof(Index));
    }

}
