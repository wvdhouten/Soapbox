namespace Soapbox.Web.Account.Password.ChangePassword;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Identity.Managers;

[Injectable]
public class ChangePasswordCommand
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly ILogger<ChangePasswordCommand> _logger;

    public ChangePasswordCommand(
        IHttpContextAccessor httpContextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        ILogger<ChangePasswordCommand> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(ChangePasswordRequest request)
    {
        var userResult = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;
        var hasPassword = await _userManager.HasPasswordAsync(user);
        var changePasswordResult = hasPassword
            ? await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword)
            : await _userManager.AddPasswordAsync(user, request.NewPassword);

        if (!changePasswordResult.Succeeded)
            return Error.ValidationError(
                "Password change failed.",
                changePasswordResult.Errors.ToDictionary(e => e.Code, e => e.Description));

        _logger.LogInformation("User changed password.");

        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }
}
