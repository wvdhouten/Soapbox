namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public partial class AccountController
{
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
        var userResult = await _userManager.GetUserAsync(User);
        if (userResult.IsFailure)
            return NotFound(userResult.Error);

        var user = userResult.Value;

        var info = await _signInManager.GetExternalLoginInfoAsync(user.Id) ?? throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");

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
        var userResult = await _userManager.GetUserAsync(User);
        if (userResult.IsFailure)
            return NotFound(userResult.Error);

        var user = userResult.Value;

        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.Count == 1 && !await _userManager.HasPasswordAsync(user))
        {
            StatusMessage = "Cannot remove the last login mechanism.";
            return RedirectToAction(nameof(Index));
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
