namespace Soapbox.Identity.Profile.UpdateProfile;
using System.Text;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Profile;
using Soapbox.Identity.Shared;

[Injectable]
public class UpdateProfileHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        IHttpContextAccessor httpContextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IEmailService emailService,
        IUrlHelper urlHelper,
        ILogger<UpdateProfileHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _urlHelper = urlHelper;
        _logger = logger;
    }

    public async Task<Result<string>> UpdateProfileAsync(UserProfile request)
    {
        var userResult = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;

        List<string> messages = [];

        if (user.DisplayName != request.DisplayName)
        {
            user.DisplayName = request.DisplayName;
            await _userManager.UpdateAsync(user);

            messages.Add("Display name updated.");
        }

        var email = await _userManager.GetEmailAsync(user);
        if (request.NewEmail != email && !string.IsNullOrEmpty(request.NewEmail))
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = _urlHelper.Action("Account", "ConfirmEmailChange", new { userId, email = request.NewEmail, code });

            try
            {
                await _emailService.SendEmailAsync(request.NewEmail, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });
                messages.Add("Email updated. Please check your email for a confirmation link.");
            }
            catch
            {
                messages.Add("Email updated, but confirmation email was not sent, please retry.");
            }
        }

        await _signInManager.RefreshSignInAsync(user);

        return string.Join(Environment.NewLine, messages);
    }
}
