namespace Soapbox.Identity.Registration.RegisterAccount;
using System.Text;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;
using Soapbox.Identity.Shared;

[Injectable]
public class RegisterAccountHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;
    private readonly ILogger<RegisterAccountHandler> _logger;

    public RegisterAccountHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IEmailService emailService,
        IUrlHelper urlHelper,
        ILogger<RegisterAccountHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _urlHelper = urlHelper;
        _logger = logger;
    }

    public async Task<Result<bool>> HandleAsync(RegisterAccountRequest request, string returnUrl)
    {
        var hasUsers = _userManager.Users.Any();
        var user = new SoapboxUser
        {
            UserName = request.Username,
            Email = request.Email,
            Role = !hasUsers ? UserRole.Administrator : UserRole.Subscriber
        };
        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
            return Error.ValidationError(
                "Account creation failed.",
                createUserResult.Errors.ToDictionary(e => e.Code, e => e.Description));

        _logger.LogInformation("New account registered with password.");

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = _urlHelper.Action("Account", "ConfirmEmail", new { userId = user.Id, code, returnUrl });
        try
        {
            await _emailService.SendEmailAsync(request.Email, "Confirm your email", new ConfirmEmail { CallbackUrl = callbackUrl });
        }
        catch
        {
            _logger.LogWarning("Unable to send confirmation email.");
        }

        if (!_userManager.Options.SignIn.RequireConfirmedAccount)
            return true;

        await _signInManager.SignInAsync(user, isPersistent: false);
        return false;
    }
}
