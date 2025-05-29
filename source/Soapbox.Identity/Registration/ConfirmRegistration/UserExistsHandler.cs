namespace Soapbox.Identity.Registration.ConfirmRegistration;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

[Injectable]
public class UserExistsHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public UserExistsHandler(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> UserWithEmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Error.NotFound($"Unable to load user with email '{email}'.");

        return Result.Success();
    }
}
