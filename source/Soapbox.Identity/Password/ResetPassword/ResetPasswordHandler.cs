namespace Soapbox.Identity.Password.ResetPassword;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using System.Text;

[Injectable]
public class ResetPasswordHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public ResetPasswordHandler(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Error.NotFound("User was not found.");

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
        var result = await _userManager.ResetPasswordAsync(user, code, request.Password);
        if (!result.Succeeded)
            return Error.ValidationError("Could not reset password.", result.Errors.ToDictionary(e => e.Code, e => e.Description));

        return Result.Success();
    }
}
