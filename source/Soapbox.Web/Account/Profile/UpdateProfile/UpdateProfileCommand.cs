namespace Soapbox.Web.Account.Profile.UpdateProfile;

using System.Text;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Application.Email.Abstractions;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Account;
using Soapbox.Web.Account.Shared;
using Soapbox.Web.Identity.Managers;
using Soapbox.Web.Models.Account;

[Injectable]
public class UpdateProfileCommand
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<UpdateProfileCommand> _logger;

    public UpdateProfileCommand(
        IHttpContextAccessor httpContextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IEmailService emailService,
        LinkGenerator linkGenerator,
        ILogger<UpdateProfileCommand> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _linkGenerator = linkGenerator;
        _logger = logger;
    }

    public async Task<Result<string>> HandleAsync(ProfileModel model)
    {
        var userResult = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;

        List<string> messages = [];

        if (user.DisplayName != model.DisplayName)
        {
            user.DisplayName = model.DisplayName;
            await _userManager.UpdateAsync(user);

            messages.Add("Display name updated.");
        }

        var email = await _userManager.GetEmailAsync(user);
        if (model.NewEmail != email && !string.IsNullOrEmpty(model.NewEmail))
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = _linkGenerator.GetPathByAction(nameof(AccountController.ConfirmEmailChange), "Account", values: new { userId, email = model.NewEmail, code })!;

            try
            {
                await _emailService.SendEmailAsync(model.NewEmail, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });
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
