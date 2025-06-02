namespace Soapbox.Identity.Mfa.ForgetClient;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class ForgetClientHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly ILogger<ForgetClientHandler> _logger;

    public ForgetClientHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        ILogger<ForgetClientHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Result<bool>> ForgetClientAsync()
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        await _signInManager.ForgetTwoFactorClientAsync();

        return true;
    }
}
