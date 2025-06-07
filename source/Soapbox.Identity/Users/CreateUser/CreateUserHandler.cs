namespace Soapbox.Identity.Users.CreateUser;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Soapbox.Domain.Email;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using Soapbox.Identity.EmailContent;
using Soapbox.Identity.Helpers;
using System.Text;

[Injectable]
public class CreateUserHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUrlHelper _urlHelper;

    public CreateUserHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        IEmailService emailService,
        IUrlHelper urlHelper)
    {
        _userManager = userManager;
        _emailService = emailService;
        _urlHelper = urlHelper;
    }

    public async Task<Result> CreateUserAsync(CreateUserRequest request)
    {
        var user = new SoapboxUser
        {
            UserName = request.Username,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Role = request.Role
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            return Error.ValidationError("User creation failed", result.Errors.ToDictionary(e => e.Code, e => e.Description));

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = _urlHelper.Action("Account", "ResetPassword", new { area = string.Empty, code })!;
        await _emailService.SendEmailAsync(user.Email, "Reset Password", new ResetPassword { CallbackUrl = callbackUrl });

        return Result.Success();
    }
}
