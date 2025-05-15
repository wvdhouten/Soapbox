namespace Soapbox.Web.Account.Email.ConfirmEmail;

using System.Text;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Identity.Managers;

[Injectable]
public class ConfirmEmailCommand
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public ConfirmEmailCommand(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> HandleAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound($"Unable to load user with ID '{userId}'.");

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
            return Result.Success();
        else
            return Error.Unknown();
    }
}
