namespace Soapbox.Identity.Email.SendEmailVerification;
using System.Text;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Shared;

[Injectable]
public class SendEmailVerificationHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;

    public SendEmailVerificationHandler(TransactionalUserManager<SoapboxUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IUrlHelper urlHelper)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _urlHelper = urlHelper;
    }

    public async Task<Result> SendEmailVerificationEmail(SendEmailVerificationRequest request)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user is null)
            return Error.NotFound("User not found.");

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = _urlHelper.Action("Account", nameof(ConfirmEmail), new { userId, code });

        await _emailService.SendEmailAsync(request.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });

        return Result.Success("Email verification sent successfully.");
    }
}
