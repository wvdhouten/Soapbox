namespace Soapbox.Identity.Authentication.ExternalLoginRegistration;
using System.Text;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Shared;

[Injectable]
public class ExternalLoginRegistrationHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;

    public ExternalLoginRegistrationHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IEmailService emailService,
        IUrlHelper urlHelper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _urlHelper = urlHelper;
    }

    public async Task<Result> RegisterExternalLogin(ExternalLoginRegistrationRequest request)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Error.NotFound("Error loading external login information.");

        var hasUsers = _userManager.Users.Any();
        var user = new SoapboxUser
        {
            UserName = request.Username,
            Email = request.Email,
            Role = !hasUsers ? UserRole.Administrator : UserRole.Subscriber
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            return Error.ValidationError("User creation failed.", result.Errors.ToDictionary(e => e.Code, e => e.Description));

        result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
            return Error.ValidationError("Adding login provider failed.", result.Errors.ToDictionary(e => e.Code, e => e.Description));

        
        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            await SendAccountConfirmationEmailAsync(request, user);
            return new ExternalLoginRegistrationResult(true);
        }

        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return new ExternalLoginRegistrationResult(false);
    }

    private async Task SendAccountConfirmationEmailAsync(ExternalLoginRegistrationRequest request, SoapboxUser user)
    {
        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = _urlHelper.Action("Account", nameof(ConfirmEmail), new { userId, code });

        await _emailService.SendEmailAsync(request.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl! });
    }
}
