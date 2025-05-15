namespace Soapbox.Web.Account.Password.GetPasswordStatus;

using Alkaline64.Injectable;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Identity.Managers;

[Injectable]
public class GetPasswordStatusQuery
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public GetPasswordStatusQuery(IHttpContextAccessor httpContextAccessor, TransactionalUserManager<SoapboxUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<Result<PasswordStatus>> HandleAsync()
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
