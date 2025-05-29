namespace Soapbox.Identity.Authentication.ForgotPassword;
using System.Text;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Shared;

[Injectable]
public class ForgotPasswordHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;

    public ForgotPasswordHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        IEmailService emailService,
        IUrlHelper urlHelper)
    {
        _userManager = userManager;
        _emailService = emailService;
        _urlHelper = urlHelper;
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            return Error.NotFound("User was not found.");

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = _urlHelper.Action("Account", nameof(ResetPassword), new { code });
        await _emailService.SendEmailAsync(request.Email, "Reset Password", new ResetPassword { CallbackUrl = callbackUrl });

        return Result.Success();
    }
}
