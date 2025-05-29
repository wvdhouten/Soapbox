namespace Soapbox.Identity.Email.ConfirmEmail;
using System.Text;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class ConfirmEmailChangeHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;

    public ConfirmEmailChangeHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result> ConfirmEmailChange(string userId, string email, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("User not found");

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
            return Error.Unknown();

        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }
}
