namespace Soapbox.Identity.Password.GetPasswordStatus;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class GetPasswordStatusHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public GetPasswordStatusHandler(IHttpContextAccessor httpContextAccessor, TransactionalUserManager<SoapboxUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<Result<PasswordStatus>> GetPasswordStatusAsync()
    {
        var userResult = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        var model = new PasswordStatus
        {
            HasPassword = await _userManager.HasPasswordAsync(userResult.Value)
        };

        return model;
    }
}
